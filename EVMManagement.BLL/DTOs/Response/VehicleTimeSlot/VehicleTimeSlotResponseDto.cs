using System;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Response.VehicleTimeSlot
{
    public class VehicleTimeSlotResponseDto
    {
        public Guid Id { get; set; }
        public Guid VehicleId { get; set; }
        public Guid DealerId { get; set; }
        public Guid MasterSlotId { get; set; }
        public DateTime SlotDate { get; set; }
        public TimeSlotStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        // nested
        public EVMManagement.BLL.DTOs.Response.Vehicle.VehicleResponseDto? Vehicle { get; set; }
        public EVMManagement.BLL.DTOs.Response.Dealer.DealerResponseDto? Dealer { get; set; }
        public EVMManagement.BLL.DTOs.Response.MasterTimeSlot.MasterTimeSlotResponseDto? MasterSlot { get; set; }
    }
}

