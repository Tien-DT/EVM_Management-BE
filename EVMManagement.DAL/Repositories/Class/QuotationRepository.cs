using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Class
{
    public class QuotationRepository : GenericRepository<Quotation>, IQuotationRepository
    {
        public QuotationRepository(AppDbContext context) : base(context)
        {
        }

        public IQueryable<Quotation> GetByCustomerId(Guid customerId)
        {
            return _dbSet.Where(q => q.CustomerId == customerId && !q.IsDeleted);
        }

        public IQueryable<Quotation> GetByDealerId(Guid dealerId)
        {
            return _dbSet
                .Include(q => q.Order)
                .Where(q => !q.IsDeleted && q.Order != null && q.Order.DealerId == dealerId);
        }
    }
}
