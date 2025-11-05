using System;
using System.Linq;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IQuotationDetailRepository : IGenericRepository<QuotationDetail>
    {
        IQueryable<QuotationDetail> GetByQuotationId(Guid quotationId);
    }
}
