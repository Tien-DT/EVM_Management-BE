using EVMManagement.DAL.Models.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IHandoverRecordRepository : IGenericRepository<HandoverRecord>
    {
        IQueryable<HandoverRecord> GetQueryableWithIncludes();
        Task<HandoverRecord?> GetByIdWithIncludesAsync(Guid id);
    }
}
