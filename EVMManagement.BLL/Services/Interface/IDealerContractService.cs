using EVMManagement.BLL.DTOs.Request.DealerContract;
using EVMManagement.BLL.DTOs.Response.DealerContract;
using EVMManagement.BLL.DTOs.Response;
using System;
using System.Threading.Tasks;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IDealerContractService
    {
    Task<DealerContractResponseDto> CreateAsync(DealerContractCreateDto dto);
        Task<PagedResult<DealerContractResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<DealerContractResponseDto?> GetByDealerIdAsync(Guid dealerId);
    Task<DealerContractResponseDto?> GetByIdAsync(Guid id);
    
    // Send OTP to dealer's registered email. OTP is cached server-side for short period.
    Task<bool> SendOtpAsync(Guid dealerId);
    
    // Verify OTP previously sent for dealer
    Task<bool> VerifyOtpAsync(Guid dealerId, string otp);
    }
}
