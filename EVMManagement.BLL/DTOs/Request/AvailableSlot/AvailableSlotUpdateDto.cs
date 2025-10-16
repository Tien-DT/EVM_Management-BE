using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.AvailableSlot
{
    public class AvailableSlotUpdateDto
    {
        [Required(ErrorMessage = "VehicleId is required")]
        public Guid VehicleId { get; set; }

        [Required(ErrorMessage = "DealerId is required")]
        public Guid DealerId { get; set; }

        [Required(ErrorMessage = "MasterSlotId is required")]
        public Guid MasterSlotId { get; set; }

        [Required(ErrorMessage = "SlotDate is required")]
        public DateTime SlotDate { get; set; }

        public bool IsAvailable { get; set; }
    }
}

