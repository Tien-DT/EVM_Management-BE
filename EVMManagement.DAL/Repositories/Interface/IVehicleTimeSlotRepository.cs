using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IVehicleTimeSlotRepository : IGenericRepository<VehicleTimeSlot>
    {       
        Task<Dictionary<DateTime, List<(Guid MasterSlotId, int StartOffsetMinutes, int DurationMinutes, int AvailableCount)>>> 
            GetSlotSummariesByModelAsync(Guid modelId, Guid dealerId, DateTime? fromDate, DateTime? toDate);
        
        Task<List<(Guid VehicleId, string Vin)>> 
            GetAvailableVehiclesBySlotAsync(Guid modelId, Guid dealerId, DateTime slotDate, Guid masterSlotId);

        Task<List<Guid>> GetExistingVehicleIdsForSlotDateAsync(IEnumerable<Guid> vehicleIds, DateTime slotDate);
        
        Task<List<Vehicle>> GetVehiclesByIdsAsync(IEnumerable<Guid> vehicleIds);
        
        Task BulkAddAsync(IEnumerable<VehicleTimeSlot> slots);

        Task<List<VehicleTimeSlot>> GetSlotsByDateRangeAsync(
            Guid dealerId, 
            DateTime fromDate, 
            DateTime toDate, 
            TimeSlotStatus? status = null
            );
    }
}

