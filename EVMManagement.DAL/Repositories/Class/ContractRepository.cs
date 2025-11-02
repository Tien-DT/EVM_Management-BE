using System;
using System.Linq;
using System.Threading.Tasks;
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

        public IQueryable<Contract> GetContractsWithDetails(Guid? orderId, Guid? customerId, Guid? dealerId, Guid? createdByUserId, Guid? signedByUserId, ContractStatus? status, ContractType? contractType)
        {
            var query = _dbSet
                .Include(c => c.Order)
                .Include(c => c.Customer)
                .Include(c => c.Dealer)
                .Include(c => c.CreatedByUser)
                .Include(c => c.SignedByUser)
                .Where(c => !c.IsDeleted);

            if (orderId.HasValue)
            {
                query = query.Where(c => c.OrderId == orderId.Value);
            }

            if (customerId.HasValue)
            {
                query = query.Where(c => c.CustomerId == customerId.Value);
            }

            if (dealerId.HasValue)
            {
                query = query.Where(c =>
                    c.DealerId == dealerId.Value ||
                    (c.Order != null && c.Order.DealerId == dealerId.Value));
            }

            if (createdByUserId.HasValue)
            {
                query = query.Where(c => c.CreatedByUserId == createdByUserId.Value);
            }

            if (signedByUserId.HasValue)
            {
                query = query.Where(c => c.SignedByUserId == signedByUserId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            if (contractType.HasValue)
            {
                query = query.Where(c => c.ContractType == contractType.Value);
            }

            return query;
        }

        public IQueryable<Contract> GetContractsByDealerId(Guid dealerId, ContractStatus? status)
        {
            var query = _dbSet
                .Include(c => c.Order)
                    .ThenInclude(o => o.Dealer)
                .Include(c => c.Customer)
                .Include(c => c.Dealer)
                .Include(c => c.CreatedByUser)
                .Include(c => c.SignedByUser)
                .Where(c => !c.IsDeleted && (c.DealerId == dealerId || c.Order.DealerId == dealerId));

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            return query;
        }

        public async Task<Contract?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(c => c.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.VehicleVariant)
                            .ThenInclude(vv => vv.VehicleModel)
                .Include(c => c.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.Vehicle)
                .Include(c => c.Customer)
                .Include(c => c.Dealer)
                .Include(c => c.CreatedByUser)
                .Include(c => c.SignedByUser)
                .Include(c => c.DigitalSignatures)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }
    }
}
