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

        public async Task<List<Guid>> GetExistingVehicleIdsForSlotDateAsync(IEnumerable<Guid> vehicleIds, DateTime slotDate)
        {
            var targetDate = slotDate.Date;
            return await _dbSet
                .Where(s => vehicleIds.Contains(s.VehicleId) 
                            && s.SlotDate.Date == targetDate 
                            && !s.IsDeleted)
                .Select(s => s.VehicleId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<Vehicle>> GetVehiclesByIdsAsync(IEnumerable<Guid> vehicleIds)
        {
            return await _context.Vehicles
                .Where(v => vehicleIds.Contains(v.Id) && !v.IsDeleted)
                .ToListAsync();
        }

        public async Task BulkAddAsync(IEnumerable<VehicleTimeSlot> slots)
        {
            await _dbSet.AddRangeAsync(slots);
        }

        public async Task<List<VehicleTimeSlot>> GetSlotsByDateRangeAsync(
            Guid dealerId,
            DateTime fromDate,
            DateTime toDate,
            TimeSlotStatus? status = null
            )
        {
            var query = _dbSet
                .Include(vts => vts.MasterSlot)
                .Include(vts => vts.Vehicle)
                    .ThenInclude(v => v.VehicleVariant)
                        .ThenInclude(vv => vv.VehicleModel)
                .Where(vts => vts.DealerId == dealerId
                           && vts.SlotDate.Date >= fromDate.Date
                           && vts.SlotDate.Date <= toDate.Date
                           && !vts.IsDeleted);

            // Apply optional filters
            if (status.HasValue)
            {
                query = query.Where(vts => vts.Status == status.Value);
            }

           

            // Execute query with ordering
            return await query
                .OrderBy(vts => vts.SlotDate)
                .ThenBy(vts => vts.MasterSlot.StartOffsetMinutes)
                .ToListAsync();
        }

        public async Task<List<Guid>> GetAssignedVehicleIdsForSlotAsync(Guid dealerId, DateTime slotDate, Guid masterSlotId)
        {
            var targetDate = slotDate.Date;
            return await _dbSet
                .Where(s => s.DealerId == dealerId
                            && s.SlotDate.Date == targetDate
                            && s.MasterSlotId == masterSlotId
                            && !s.IsDeleted)
                .Select(s => s.VehicleId)
                .Distinct()
                .ToListAsync();
        }
    }
}

