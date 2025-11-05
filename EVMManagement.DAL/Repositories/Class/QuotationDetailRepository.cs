using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Class
{
    public class QuotationDetailRepository : GenericRepository<QuotationDetail>, IQuotationDetailRepository
    {
        public QuotationDetailRepository(AppDbContext context) : base(context)
        {
        }

        public IQueryable<QuotationDetail> GetByQuotationId(Guid quotationId)
        {
            return _dbSet
                .Include(qd => qd.VehicleVariant)
                    .ThenInclude(vv => vv.VehicleModel)
                .Include(qd => qd.Quotation.Order)
                .Where(qd => qd.QuotationId == quotationId && !qd.IsDeleted);
        }
    }
}
