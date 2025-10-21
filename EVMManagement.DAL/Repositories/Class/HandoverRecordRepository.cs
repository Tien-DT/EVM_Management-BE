using Microsoft.EntityFrameworkCore;
using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Class
{
    public class HandoverRecordRepository : GenericRepository<HandoverRecord>, IHandoverRecordRepository
    {
        public HandoverRecordRepository(AppDbContext context) : base(context)
        {
        }

        public IQueryable<HandoverRecord> GetQueryableWithIncludes()
        {
            return _dbSet
                .Include(x => x.Order)
                .Include(x => x.Vehicle)
                .Include(x => x.TransportDetail);
                
        }

        public async Task<HandoverRecord?> GetByIdWithIncludesAsync(Guid id)
        {
            return await GetQueryableWithIncludes().FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
