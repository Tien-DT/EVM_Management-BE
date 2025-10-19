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
using EVMManagement.BLL.DTOs.Request.DigitalSignature;
using EVMManagement.BLL.DTOs.Response.DigitalSignature;

namespace EVMManagement.BLL.Services.Class
{
    public class DealerContractService : IDealerContractService
    {
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserProfileService _userProfileService;
    private readonly IDigitalSignatureService _digitalSignatureService;

        public DealerContractService(IUnitOfWork unitOfWork, IUserProfileService userProfileService, IDigitalSignatureService digitalSignatureService)
        {
            _unitOfWork = unitOfWork;
            _userProfileService = userProfileService;
            _digitalSignatureService = digitalSignatureService;
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
                SignedByDealerUserId = null,
                SignedByEvmUserId = null,
                ContractLink = dto.ContractLink
            };

          
            if (signAsEvm && evmSignerAccountId.HasValue)
            {
                var signerProfile = await _userProfileService.GetByAccountIdAsync(evmSignerAccountId.Value);
                if (signerProfile != null)
                {
                    entity.SignedByEvmUserId = signerProfile.Id;
                    entity.SignedAt = DateTime.UtcNow;
                    entity.Status = DealerContractStatus.ACTIVE;
                }
            }

            await _unitOfWork.DealerContracts.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(entity);
        }



    public async Task<bool> MarkAsSignedAsync(Guid dealerId, string otp, string signerEmail)
        {
            
            var contract = await _unitOfWork.DealerContracts.GetQueryable()
                        .Where(d => d.DealerId == dealerId)
                        .OrderByDescending(d => d.CreatedDate)
                        .FirstOrDefaultAsync();

            if (contract == null) return false;

            var verifyDto = new VerifyOtpDto
            {
                SignerEmail = signerEmail,
                EntityType = SignatureEntityType.DEALER_CONTRACT,
                DealerContractId = contract.Id,
                OtpCode = otp
            };

            DigitalSignatureResponse dsResult;
            try
            {
                dsResult = await _digitalSignatureService.VerifyOtpAsync(verifyDto);
            }
            catch
            {
                return false;
            }

            if (dsResult == null || dsResult.Status != SignatureStatus.OTP_VERIFIED)
            {
                return false;
            }

            contract.Status = DealerContractStatus.ACTIVE;
            contract.SignedAt = DateTime.UtcNow;

           
            var managerProfile = await _userProfileService.GetManagerByDealerIdAsync(dealerId);
            if (managerProfile != null && !string.IsNullOrWhiteSpace(managerProfile.Account?.Email) && managerProfile.Account.Email.Equals(signerEmail, StringComparison.OrdinalIgnoreCase))
            {
                contract.SignedByDealerUserId = managerProfile.Id;
            }

            _unitOfWork.DealerContracts.Update(contract);
            await _unitOfWork.SaveChangesAsync();

            return true;
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
