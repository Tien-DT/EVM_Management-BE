using System;
using System.Linq;
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
    }
}
