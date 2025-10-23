using EVMManagement.DAL.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IVehicleTimeSlotRepository : IGenericRepository<VehicleTimeSlot>
    {
        /// <summary>
        /// Get all vehicles of a variant at a dealer
        /// </summary>
        Task<List<(Guid Id, string Vin)>> GetAvailableVehiclesByVariantAndDealerAsync(Guid variantId, Guid dealerId);

        /// <summary>
        /// Get booked/available VehicleTimeSlots for a variant in date range
        /// </summary>
        Task<List<VehicleTimeSlot>> GetSlotsByVariantInDateRangeAsync(Guid modelId, Guid variantId, Guid dealerId, DateTime? fromDate, DateTime? toDate);

        /// <summary>
        /// Get available VehicleTimeSlots for a specific vehicle in date range
        /// </summary>
        Task<List<VehicleTimeSlot>> GetAvailableSlotsByVehicleInDateRangeAsync(Guid vehicleId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get available VehicleTimeSlots by date (for scheduling screen)
        /// </summary>
        Task<List<VehicleTimeSlot>> GetAvailableSlotsByDateAsync(Guid dealerId, DateTime slotDate);

        /// <summary>
        /// Get VehicleTimeSlots for a specific slot and date with full details
        /// </summary>
        Task<List<VehicleTimeSlot>> GetSlotsByDateAndMasterSlotAsync(Guid modelId, Guid variantId, Guid dealerId, DateTime slotDate, Guid masterSlotId);
    }
}


