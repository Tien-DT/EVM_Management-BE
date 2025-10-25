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
    public class VehicleTimeSlotRepository : GenericRepository<VehicleTimeSlot>, IVehicleTimeSlotRepository
    {
        public VehicleTimeSlotRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Dictionary<DateTime, List<(Guid MasterSlotId, int StartOffsetMinutes, int DurationMinutes, int AvailableCount)>>> 
            GetSlotSummariesByModelAsync(Guid modelId, Guid dealerId, DateTime? fromDate, DateTime? toDate)
        {
            var results = await _dbSet
                .Include(s => s.MasterSlot)
                .Include(s => s.Vehicle)
                    .ThenInclude(v => v.VehicleVariant)
                .Where(s => !s.IsDeleted
                            && s.DealerId == dealerId
                            && (!fromDate.HasValue || s.SlotDate.Date >= fromDate.Value.Date)
                            && (!toDate.HasValue || s.SlotDate.Date <= toDate.Value.Date)
                            && s.Vehicle.VehicleVariant.ModelId == modelId
                            && s.Vehicle.Status == VehicleStatus.IN_STOCK
                            && s.Vehicle.Purpose == VehiclePurpose.TEST_DRIVE
                            && s.Status == TimeSlotStatus.AVAILABLE)
                .GroupBy(s => new { s.SlotDate.Date, s.MasterSlotId, s.MasterSlot.StartOffsetMinutes, s.MasterSlot.DurationMinutes })
                .Select(g => new
                {
                    SlotDate = g.Key.Date,
                    MasterSlotId = g.Key.MasterSlotId,
                    StartOffsetMinutes = g.Key.StartOffsetMinutes ?? 0,
                    DurationMinutes = g.Key.DurationMinutes ?? 0,
                    AvailableCount = g.Count()
                })
                .OrderBy(x => x.SlotDate)
                .ThenBy(x => x.StartOffsetMinutes)
                .ToListAsync();

            // Group by date and create dictionary
            return results
                .GroupBy(r => r.SlotDate)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => (x.MasterSlotId, x.StartOffsetMinutes, x.DurationMinutes, x.AvailableCount)).ToList()
                );
        }

        public async Task<List<(Guid VehicleId, string Vin)>> 
            GetAvailableVehiclesBySlotAsync(Guid modelId, Guid dealerId, DateTime slotDate, Guid masterSlotId)
        {
            var targetDate = slotDate.Date;
            
            return await _dbSet
                .Include(s => s.Vehicle)
                    .ThenInclude(v => v.VehicleVariant)
                .Where(s => !s.IsDeleted
                            && s.DealerId == dealerId
                            && s.SlotDate.Date == targetDate
                            && s.MasterSlotId == masterSlotId
                            && s.Vehicle.VehicleVariant.ModelId == modelId
                            && s.Vehicle.Status == VehicleStatus.IN_STOCK
                            && s.Vehicle.Purpose == VehiclePurpose.TEST_DRIVE
                            && s.Status == TimeSlotStatus.AVAILABLE)
                .Select(s => new { s.VehicleId, s.Vehicle.Vin })
                .Distinct()
                .ToListAsync()
                .ContinueWith(t => t.Result.Select(x => (x.VehicleId, x.Vin)).ToList());
        }
      
    }
}


