using System;

namespace EVMManagement.BLL.DTOs.Response.AvailableSlot
{
    public class AvailableSlotResponseDto
    {
        public Guid Id { get; set; }
        public Guid VehicleId { get; set; }
        public Guid DealerId { get; set; }
        public Guid MasterSlotId { get; set; }
        public DateTime SlotDate { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}

