using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Contract;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Contract;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IContractService
    {
        Task<Contract> CreateContractAsync(ContractCreateDto dto);
        Task<PagedResult<ContractResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<ContractResponse?> GetByIdAsync(Guid id);
        Task<ContractResponse?> UpdateAsync(Guid id, ContractUpdateDto dto);
        Task<ContractResponse?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<bool> DeleteAsync(Guid id);
    }
}
