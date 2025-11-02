using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Class
{
    public class WarehouseRepository : GenericRepository<Warehouse>, IWarehouseRepository
    {
        private readonly AppDbContext _context;

        public WarehouseRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Warehouse> GetAllAsync()
        {
            return _dbSet
                .Include(w => w.Dealer)
                .Include(w => w.Vehicles)
                .ThenInclude(v => v.VehicleVariant)
                .ThenInclude(vv => vv.VehicleModel);
        }

        public async Task<Warehouse?> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(w => w.Dealer)
                .Include(w => w.Vehicles)
                     .ThenInclude(v => v.VehicleVariant)
                         .ThenInclude(vv => vv.VehicleModel)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public IQueryable<Warehouse> GetWarehousesByDealerIdAsync(Guid dealerId)
        {
            return _dbSet
                .Where(w => w.DealerId == dealerId)
                .Include(w => w.Dealer)
                .Include(w => w.Vehicles)
                    .ThenInclude(v => v.VehicleVariant)
                        .ThenInclude(vv => vv.VehicleModel);
        }

        public IQueryable<Warehouse> GetWarehousesByType(WarehouseType type)
        {
            return _dbSet
                .Where(w => w.Type == type)
                .Include(w => w.Dealer)
                .Include(w => w.Vehicles)
                    .ThenInclude(v => v.VehicleVariant)
                        .ThenInclude(vv => vv.VehicleModel);
        }
    }
}
