using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Deposit;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Deposit;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IDepositService
    {
        Task<DepositResponse> CreateAsync(DepositCreateDto dto);
        Task<DepositResponse> CreatePreOrderDepositAsync(PreOrderDepositRequestDto dto);
        Task<PagedResult<DepositResponse>> GetAsync(Guid? orderId, Guid? receivedByUserId, int pageNumber, int pageSize);
        Task<DepositResponse?> GetByIdAsync(Guid id);
        Task<DepositResponse?> UpdateAsync(Guid id, DepositUpdateDto dto);
        Task<bool> SoftDeleteAsync(Guid id);
    }
}
