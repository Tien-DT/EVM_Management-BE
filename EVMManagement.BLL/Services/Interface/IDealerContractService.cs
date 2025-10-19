using EVMManagement.BLL.DTOs.Request.DealerContract;
using EVMManagement.BLL.DTOs.Response.DealerContract;
using EVMManagement.BLL.DTOs.Response;
using System;
using System.Threading.Tasks;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IDealerContractService
    {
    Task<DealerContractResponseDto> CreateAsync(DealerContractCreateDto dto, Guid? evmSignerAccountId = null, bool signAsEvm = false);
    Task<PagedResult<DealerContractResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<DealerContractResponseDto?> GetByDealerIdAsync(Guid dealerId);
    Task<DealerContractResponseDto?> GetByIdAsync(Guid id);
    // Send OTP for dealer associated with caller's accountId. Service will resolve dealerId from account.
    Task<bool> SendOtpAsync(Guid accountId, string? recipientEmail = null);
    Task<bool> VerifyOtpAsync(Guid dealerId, string otp, Guid? signerAccountId = null);
    }
}
