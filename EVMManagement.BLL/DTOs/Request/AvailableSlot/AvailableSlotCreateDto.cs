using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.AvailableSlot
{
    public class AvailableSlotCreateDto
    {
        [Required]
        public Guid VehicleId { get; set; }

        [Required]
        public Guid DealerId { get; set; }

        [Required]
        public Guid MasterSlotId { get; set; }

        [Required]
        public DateTime SlotDate { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}

