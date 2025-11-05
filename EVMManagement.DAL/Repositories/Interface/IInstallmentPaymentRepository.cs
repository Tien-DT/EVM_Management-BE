using System;
using System.Linq;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IInstallmentPaymentRepository : IGenericRepository<InstallmentPayment>
    {
        IQueryable<InstallmentPayment> GetByPlanId(Guid planId);
    }
}
