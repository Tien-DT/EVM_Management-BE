using EVMManagement.BLL.DTOs.Request.DealerContract;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.DealerContract;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.Services.Class
{
    public class DealerContractService : IDealerContractService
    {
    private readonly IUnitOfWork _unitOfWork;
    private readonly Services.Interface.IEmailService _emailService;
    private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _distributedCache;
    private readonly Services.Interface.IUserProfileService _userProfileService;

        public DealerContractService(IUnitOfWork unitOfWork, Services.Interface.IEmailService emailService, Microsoft.Extensions.Caching.Distributed.IDistributedCache distributedCache, Services.Interface.IUserProfileService userProfileService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _distributedCache = distributedCache;
            _userProfileService = userProfileService;
        }

        public async Task<DealerContractResponseDto> CreateAsync(DealerContractCreateDto dto, Guid? evmSignerAccountId = null, bool signAsEvm = false)
        {
            var entity = new DealerContract
            {
                DealerId = dto.DealerId,
                ContractCode = dto.ContractCode,
                Terms = dto.Terms,
                Status = dto.Status,
                EffectiveDate = dto.EffectiveDate,
                ExpirationDate = dto.ExpirationDate,
                // Do not trust client-provided signer ids. EVM signer (if requested) will be resolved server-side.
                SignedByDealerUserId = null,
                SignedByEvmUserId = null,
                ContractLink = dto.ContractLink
            };

            // If caller requested to sign as EVM admin, resolve account -> userProfile and set SignedByEvmUserId
            if (signAsEvm && evmSignerAccountId.HasValue)
            {
                var signerProfile = await _userProfileService.GetByAccountIdAsync(evmSignerAccountId.Value);
                if (signerProfile != null)
                {
                    entity.SignedByEvmUserId = signerProfile.Id;
                    entity.SignedAt = DateTime.UtcNow;
                    // Also consider setting status to ACTIVE if appropriate
                    entity.Status = EVMManagement.DAL.Models.Enums.DealerContractStatus.ACTIVE;
                }
            }

            await _unitOfWork.DealerContracts.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(entity);
        }

        public async Task<bool> SendOtpAsync(Guid accountId, string? recipientEmail = null)
        {
            // Resolve user profile and dealerId from caller's accountId
            var profile = await _userProfileService.GetByAccountIdAsync(accountId);
            if (profile == null || !profile.DealerId.HasValue) return false;

            var dealerId = profile.DealerId.Value;
            var dealer = await _unitOfWork.Dealers.GetByIdAsync(dealerId);
            if (dealer == null) return false;

            var manager = await _userProfileService.GetManagerByDealerIdAsync(dealerId);
            string recipientName = dealer.Name ?? string.Empty;

            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                if (manager != null && manager.Account != null && !string.IsNullOrWhiteSpace(manager.Account.Email))
                {
                    recipientEmail = manager.Account.Email;
                    recipientName = manager.FullName ?? manager.Account.Email;
                }
                else if (!string.IsNullOrWhiteSpace(dealer.Email))
                {
                    recipientEmail = dealer.Email;
                    recipientName = dealer.Name ?? dealer.Email;
                }
            }
            else
            {
                recipientName = manager?.FullName ?? dealer.Name ?? recipientEmail;
            }

            if (string.IsNullOrWhiteSpace(recipientEmail)) return false;

            var rng = new System.Random();
            var otp = rng.Next(100000, 999999).ToString();

            var cacheKey = $"DealerOtp:{dealerId}";
            var options = new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = System.TimeSpan.FromMinutes(5)
            };
            await _distributedCache.SetStringAsync(cacheKey, otp, options);
            var subject = Templates.EmailTemplates.Subjects.Otp;
            var body = Templates.EmailTemplates.OtpEmail(recipientName, otp, 5);

            try
            {
                await _emailService.SendEmailAsync(recipientEmail, subject, body, isHtml: true);
                return true;
            }
            catch
            {
                await _distributedCache.RemoveAsync(cacheKey);
                return false;
            }
        }

    public async Task<bool> VerifyOtpAsync(Guid dealerId, string otp, Guid? signerAccountId = null)
        {
            var cacheKey = $"DealerOtp:{dealerId}";
            var stored = await _distributedCache.GetStringAsync(cacheKey);
            if (string.IsNullOrWhiteSpace(stored)) return false; 

            if (stored == otp)
            {
                await _distributedCache.RemoveAsync(cacheKey);
                var contract = await _unitOfWork.DealerContracts.GetQueryable()
                    .Where(d => d.DealerId == dealerId)
                    .OrderByDescending(d => d.CreatedDate)
                    .FirstOrDefaultAsync();

                if (contract != null)
                {
                    contract.Status = DealerContractStatus.ACTIVE;
                    contract.SignedAt = DateTime.UtcNow;
                    if (signerAccountId.HasValue)
                    {
                        var signerProfile = await _userProfileService.GetByAccountIdAsync(signerAccountId.Value);
                            if (signerProfile != null)
                            {
                                contract.SignedByDealerUserId = signerProfile.Id;
                            }
                    }
                }

                    // Fallback: assign dealer signer (manager) because OTP was sent to dealer manager's email
                    if (!contract.SignedByDealerUserId.HasValue)
                    {
                            var managerProfile = await _userProfileService.GetManagerByDealerIdAsync(dealerId);
                            if (managerProfile != null)
                            {
                                contract.SignedByDealerUserId = managerProfile.Id;
                            }
                    }
                    _unitOfWork.DealerContracts.Update(contract);
                    await _unitOfWork.SaveChangesAsync();
                

                return true;
            }

            return false;
        }

        public async Task<PagedResult<DealerContractResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.DealerContracts.GetQueryable();
            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(d => d.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = items.Select(MapToDto).ToList();
            return PagedResult<DealerContractResponseDto>.Create(responses, total, pageNumber, pageSize);
        }

        public async Task<DealerContractResponseDto?> GetByDealerIdAsync(Guid dealerId)
        {
            var entity = await _unitOfWork.DealerContracts.GetQueryable()
                .Where(d => d.DealerId == dealerId)
                .OrderByDescending(d => d.CreatedDate)
                .FirstOrDefaultAsync();

            if (entity == null) return null;
            return MapToDto(entity);
        }

        public async Task<DealerContractResponseDto?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.DealerContracts.GetByIdAsync(id);
            if (entity == null) return null;
            return MapToDto(entity);
        }

        private DealerContractResponseDto MapToDto(DealerContract e)
        {
            return new DealerContractResponseDto
            {
                Id = e.Id,
                DealerId = e.DealerId,
                ContractCode = e.ContractCode,
                Terms = e.Terms,
                Status = e.Status,
                EffectiveDate = e.EffectiveDate,
                ExpirationDate = e.ExpirationDate,
                SignedAt = e.SignedAt,
                SignedByDealerUserId = e.SignedByDealerUserId,
                SignedByEvmUserId = e.SignedByEvmUserId,
                ContractLink = e.ContractLink,
                CreatedDate = e.CreatedDate,
                ModifiedDate = e.ModifiedDate,
                DeletedDate = e.DeletedDate,
                IsDeleted = e.IsDeleted
            };
        }
    }
}
