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
    }
}
