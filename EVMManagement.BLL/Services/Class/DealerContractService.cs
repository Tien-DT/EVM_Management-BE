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

        public async Task<DealerContractResponseDto> CreateAsync(DealerContractCreateDto dto)
        {
            var entity = new DealerContract
            {
                DealerId = dto.DealerId,
                ContractCode = dto.ContractCode,
                Terms = dto.Terms,
                Status = dto.Status,
                EffectiveDate = dto.EffectiveDate,
                ExpirationDate = dto.ExpirationDate,
                SignedByDealerUserId = dto.SignedByDealerUserId,
                SignedByEvmUserId = dto.SignedByEvmUserId,
                ContractLink = dto.ContractLink
            };

            await _unitOfWork.DealerContracts.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(entity);
        }

        public async Task<bool> SendOtpAsync(Guid dealerId)
        {
            // Get dealer
            var dealer = await _unitOfWork.Dealers.GetByIdAsync(dealerId);
            if (dealer == null) return false;

            // Try find dealer manager via service (service will handle fallback logic)
            var manager = await _userProfileService.GetManagerByDealerIdAsync(dealerId);

            string? recipientEmail = null;
            string recipientName = dealer.Name;

            if (manager != null && manager.Account != null && !string.IsNullOrWhiteSpace(manager.Account.Email))
            {
                recipientEmail = manager.Account.Email;
                recipientName = manager.FullName ?? manager.Account.Email;
            }
            else if (!string.IsNullOrWhiteSpace(dealer.Email))
            {
                recipientEmail = dealer.Email;
            }

            if (string.IsNullOrWhiteSpace(recipientEmail)) return false;

            // Generate 6-digit OTP
            var rng = new System.Random();
            var otp = rng.Next(100000, 999999).ToString();

            // Cache OTP for 5 minutes keyed by dealerId (Redis/IDistributedCache)
            var cacheKey = $"DealerOtp:{dealerId}";
            var options = new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = System.TimeSpan.FromMinutes(5)
            };
            await _distributedCache.SetStringAsync(cacheKey, otp, options);

            // Compose email body using template
            var subject = Templates.EmailTemplates.Subjects.Otp;
            var body = Templates.EmailTemplates.OtpEmail(recipientName, otp, 5);

            try
            {
                await _emailService.SendEmailAsync(recipientEmail, subject, body, isHtml: true);
                return true;
            }
            catch
            {
                // On failure, remove cached OTP
                await _distributedCache.RemoveAsync(cacheKey);
                return false;
            }
        }

        public async Task<bool> VerifyOtpAsync(Guid dealerId, string otp)
        {
            var cacheKey = $"DealerOtp:{dealerId}";
            var stored = await _distributedCache.GetStringAsync(cacheKey);
            if (string.IsNullOrWhiteSpace(stored)) return false; // expired or not exists

            if (stored == otp)
            {
                // correct, remove key and return true
                await _distributedCache.RemoveAsync(cacheKey);
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
