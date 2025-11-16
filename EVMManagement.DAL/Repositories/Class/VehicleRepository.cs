using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EVMManagement.DAL.Repositories.Class
{
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(AppDbContext context) : base(context)
        {
        }

        public IQueryable<Vehicle> SearchByQueryAsync(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return GetQueryable();
            q = q.ToLower();
            return GetQueryable().Where(v => v.Vin.ToLower().Contains(q));
        }

        public IQueryable<Vehicle> GetVehiclesByDealerAndVariantAsync(Guid dealerId, Guid variantId)
        {
            return _dbSet
                .Where(v => !v.IsDeleted
                            && v.Warehouse.DealerId == dealerId
                            && v.VariantId == variantId
                            && v.Status == VehicleStatus.IN_STOCK
                            && v.Purpose == VehiclePurpose.FOR_SALE)
                .Include(v => v.Warehouse)
                .Include(v => v.VehicleVariant)
                    .ThenInclude(vv => vv.VehicleModel)
                .OrderByDescending(v => v.CreatedDate);
        }

        public async Task<List<Vehicle>> GetTestDriveVehiclesByDealerAsync(Guid dealerId)
        {
            return await _context.Vehicles
                .Include(v => v.VehicleVariant)
                    .ThenInclude(vv => vv.VehicleModel)
                .Include(v => v.Warehouse)
                .Where(v => v.Warehouse.DealerId == dealerId
                            && v.Status == VehicleStatus.IN_STOCK
                            && v.Purpose == VehiclePurpose.TEST_DRIVE
                            && !v.IsDeleted)
                .ToListAsync();
        }

        public IQueryable<Vehicle> GetVehiclesByModelInWarehouseAsync(Guid warehouseId, Guid modelId, VehiclePurpose? purpose = null, VehicleStatus? status = null)
        {
            var query = _dbSet
                .Include(v => v.VehicleVariant)
                    .ThenInclude(vv => vv.VehicleModel)
                .Include(v => v.Warehouse)
                .Where(v => !v.IsDeleted
                            && v.WarehouseId == warehouseId
                            && v.VehicleVariant.VehicleModel.Id == modelId);

            if (purpose.HasValue)
            {
                query = query.Where(v => v.Purpose == purpose.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(v => v.Status == status.Value);
            }

            return query.OrderByDescending(v => v.CreatedDate);
        }
    }
}
