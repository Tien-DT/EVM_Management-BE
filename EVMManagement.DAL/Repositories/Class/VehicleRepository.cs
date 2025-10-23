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

        public async Task<List<(Guid Id, string Name, int VariantCount)>> GetModelsByDealerAsync(Guid dealerId)
        {
            var data = await _dbSet
                .Where(v => !v.IsDeleted
                            && v.Warehouse.DealerId == dealerId
                            && v.Status == VehicleStatus.IN_STOCK
                            && v.Purpose == VehiclePurpose.TEST_DRIVE)
                .Include(v => v.VehicleVariant)
                .ThenInclude(vr => vr.VehicleModel)
                .GroupBy(v => v.VehicleVariant.VehicleModel)
                .Select(g => new 
                { 
                    Model = g.Key,
                    DistinctVariants = g.Select(v => v.VehicleVariant).Distinct().Count()
                })
                .ToListAsync();

            return data
                .Select(x => (x.Model.Id, x.Model.Name, x.DistinctVariants))
                .OrderBy(x => x.Name)
                .ToList();
        }

        public async Task<List<(Guid Id, Guid ModelId, string ModelName, int AvailableCount)>> GetVariantsByDealerAndModelAsync(Guid dealerId, Guid modelId)
        {
            var variants = await _dbSet
                .Where(v => !v.IsDeleted
                            && v.Warehouse.DealerId == dealerId
                            && v.VehicleVariant.ModelId == modelId
                            && v.Status == VehicleStatus.IN_STOCK
                            && v.Purpose == VehiclePurpose.TEST_DRIVE)
                .Include(v => v.VehicleVariant)
                .ThenInclude(vr => vr.VehicleModel)
                .GroupBy(v => v.VariantId)
                .Select(g => new 
                { 
                    VariantId = g.Key,
                    FirstVariant = g.First().VehicleVariant,
                    Count = g.Count() 
                })
                .ToListAsync();

            return variants
                .Where(x => x.FirstVariant != null && x.FirstVariant.VehicleModel != null)
                .Select(x => (x.FirstVariant.Id, x.FirstVariant.ModelId, x.FirstVariant.VehicleModel.Name, x.Count))
                .OrderBy(x => x.Item3)
                .ToList();
        }
    }
}
