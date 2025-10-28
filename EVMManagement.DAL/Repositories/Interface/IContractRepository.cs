using System;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IContractRepository : IGenericRepository<Contract>
    {
        IQueryable<Contract> GetContractsWithDetails(Guid? orderId, Guid? customerId, Guid? createdByUserId, ContractStatus? status);
        IQueryable<Contract> GetContractsByDealerId(Guid dealerId, ContractStatus? status);
        Task<Contract?> GetByIdWithDetailsAsync(Guid id);
    }
}
