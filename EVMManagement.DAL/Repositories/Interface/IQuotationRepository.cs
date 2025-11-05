using System;
using System.Linq;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IQuotationRepository : IGenericRepository<Quotation>
    {
        IQueryable<Quotation> GetByCustomerId(Guid customerId);
        IQueryable<Quotation> GetByDealerId(Guid dealerId);
    }
}
