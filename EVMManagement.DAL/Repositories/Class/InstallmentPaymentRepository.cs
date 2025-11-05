using System;
using System.Linq;
using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Class
{
    public class InstallmentPaymentRepository : GenericRepository<InstallmentPayment>, IInstallmentPaymentRepository
    {
        public InstallmentPaymentRepository(AppDbContext context) : base(context)
        {
        }

        public IQueryable<InstallmentPayment> GetByPlanId(Guid planId)
        {
            return _dbSet.Where(x => x.PlanId == planId);
        }
    }
}
