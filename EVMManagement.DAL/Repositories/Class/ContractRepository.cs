using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Class
{
    public class ContractRepository : GenericRepository<Contract>, IContractRepository
    {
        public ContractRepository(AppDbContext context) : base(context)
        {
        }

        public IQueryable<Contract> GetContractsWithDetails(Guid? orderId, Guid? customerId, Guid? createdByUserId, ContractStatus? status)
        {
            var query = _dbSet
                .Include(c => c.Order)
                .Include(c => c.Customer)
                .Include(c => c.CreatedByUser)
                .Where(c => !c.IsDeleted);

            if (orderId.HasValue)
            {
                query = query.Where(c => c.OrderId == orderId.Value);
            }

            if (customerId.HasValue)
            {
                query = query.Where(c => c.CustomerId == customerId.Value);
            }

            if (createdByUserId.HasValue)
            {
                query = query.Where(c => c.CreatedByUserId == createdByUserId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            return query;
        }
    }
}
