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
    }
}
