using System;
using System.Linq;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IContractRepository : IGenericRepository<Contract>
    {
        IQueryable<Contract> GetContractsWithDetails(Guid? orderId, Guid? customerId, Guid? createdByUserId, ContractStatus? status);
    }
}
