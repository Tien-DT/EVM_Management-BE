using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Contract;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Contract;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IContractService
    {
        Task<Contract> CreateContractAsync(ContractCreateDto dto);
        Task<PagedResult<ContractDetailResponse>> GetAllAsync(Guid? orderId, Guid? customerId, Guid? dealerId, Guid? createdByUserId, Guid? signedByUserId, ContractStatus? status, ContractType? contractType, int pageNumber = 1, int pageSize = 10);
        Task<ContractResponse?> GetByIdAsync(Guid id);
        Task<ContractResponse?> UpdateAsync(Guid id, ContractUpdateDto dto);
        Task<ContractResponse?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<bool> DeleteAsync(Guid id);
        Task<PagedResult<ContractDetailResponse>> GetByDealerIdAsync(Guid dealerId, ContractStatus? status, int pageNumber = 1, int pageSize = 10);
    }
}
