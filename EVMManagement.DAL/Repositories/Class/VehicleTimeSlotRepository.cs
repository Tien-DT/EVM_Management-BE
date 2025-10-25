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

        public async Task<List<(Guid Id, string Vin)>> GetAvailableVehiclesByVariantAndDealerAsync(Guid variantId, Guid dealerId)
        {
            // Get distinct vehicle IDs that have VehicleTimeSlots for this dealer + variant in IN_STOCK status
            return await _dbSet
                .Where(s => !s.IsDeleted
                            && s.DealerId == dealerId
                            && s.Vehicle.VehicleVariant.Id == variantId
                            && s.Vehicle.Status == VehicleStatus.IN_STOCK
                            && s.Vehicle.Purpose == VehiclePurpose.TEST_DRIVE)
                .Select(s => new { s.Vehicle.Id, s.Vehicle.Vin })
                .Distinct()
                .ToListAsync()
                .ContinueWith(t => t.Result.Select(x => (x.Id, x.Vin)).ToList());
        }

        public async Task<List<VehicleTimeSlot>> GetSlotsByVariantInDateRangeAsync(Guid modelId, Guid variantId, Guid dealerId, DateTime? fromDate, DateTime? toDate)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted
                            && s.DealerId == dealerId
                            && (!fromDate.HasValue || s.SlotDate >= fromDate.Value.Date)
                            && (!toDate.HasValue || s.SlotDate <= toDate.Value.Date)
                            && s.Vehicle.VehicleVariant.ModelId == modelId
                            && s.Vehicle.VehicleVariant.Id == variantId
                            && (s.Status == TimeSlotStatus.BOOKED || s.Status == TimeSlotStatus.AVAILABLE))
                .Include(s => s.MasterSlot)
                .ToListAsync();
        }

        public async Task<List<VehicleTimeSlot>> GetAvailableSlotsByVehicleInDateRangeAsync(Guid vehicleId, DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted
                            && s.VehicleId == vehicleId
                            && s.Status == TimeSlotStatus.AVAILABLE
                            && s.SlotDate >= fromDate.Date
                            && s.SlotDate <= toDate.Date)
                .Include(s => s.MasterSlot)
                .OrderBy(s => s.SlotDate)
                .ThenBy(s => s.MasterSlot.StartOffsetMinutes)
                .ToListAsync();
        }

        public async Task<List<VehicleTimeSlot>> GetAvailableSlotsByDateAsync(Guid dealerId, DateTime slotDate)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted
                            && s.DealerId == dealerId
                            && s.Status == TimeSlotStatus.AVAILABLE
                            && s.SlotDate == slotDate.Date)
                .Include(s => s.Vehicle)
                .ThenInclude(v => v.VehicleVariant)
                .ThenInclude(vv => vv.VehicleModel)
                .Include(s => s.MasterSlot)
                .OrderBy(s => s.MasterSlot.StartOffsetMinutes)
                .ToListAsync();
        }

        public async Task<List<VehicleTimeSlot>> GetSlotsByDateAndMasterSlotAsync(Guid modelId, Guid variantId, Guid dealerId, DateTime slotDate, Guid masterSlotId)
        {
            return await _dbSet
                .Include(s => s.Vehicle)
                .ThenInclude(v => v.VehicleVariant)
                .ThenInclude(vv => vv.VehicleModel)
                .Include(s => s.MasterSlot)
                .Where(s => !s.IsDeleted
                            && s.DealerId == dealerId
                            && s.SlotDate.Date == slotDate.Date
                            && s.MasterSlotId == masterSlotId
                            && s.Vehicle.VehicleVariant.ModelId == modelId
                            && s.Vehicle.VehicleVariant.Id == variantId
                            && (s.Status == TimeSlotStatus.BOOKED || s.Status == TimeSlotStatus.AVAILABLE))
                .ToListAsync();
        }
    }
}


