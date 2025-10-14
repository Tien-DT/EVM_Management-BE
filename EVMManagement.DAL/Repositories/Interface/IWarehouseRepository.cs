using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IWarehouseRepository : IGenericRepository<Warehouse>
    {
        IQueryable<Warehouse> GetQueryableWithIncludes();
        Task<Warehouse?> GetByIdWithIncludesAsync(Guid id);
        Task<IEnumerable<Warehouse>> GetAllWithIncludesAsync();
    }
}
