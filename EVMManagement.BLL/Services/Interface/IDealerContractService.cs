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
    Task<bool> MarkAsSignedAsync(Guid dealerId, string otp, string signerEmail);
    }
}
