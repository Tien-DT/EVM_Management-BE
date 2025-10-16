using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IWarehouseRepository : IGenericRepository<Warehouse>
    {
        IQueryable<Warehouse> GetAllAsync();
        Task<Warehouse?> GetByIdAsync(Guid id);
        IQueryable<Warehouse> GetWarehousesByDealerIdAsync(Guid dealerId);
    }
}
