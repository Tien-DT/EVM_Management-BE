using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
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

        public IQueryable<Warehouse> GetQueryableWithIncludes()
        {
            return _context.Set<Warehouse>()
                .Include(w => w.Dealer)
                .Include(w => w.Vehicles);
        }

        public async Task<Warehouse?> GetByIdWithIncludesAsync(Guid id)
        {
            return await _context.Set<Warehouse>()
                .Include(w => w.Dealer)
                .Include(w => w.Vehicles)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<IEnumerable<Warehouse>> GetAllWithIncludesAsync()
        {
            return await _context.Set<Warehouse>()
                .Include(w => w.Dealer)
                .Include(w => w.Vehicles)
                .ToListAsync();
        }
    }
}
