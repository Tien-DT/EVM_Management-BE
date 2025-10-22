using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.VehicleTimeSlot
{
    public class VehicleTimeSlotCreateDto
    {
        [Required]
        public Guid VehicleId { get; set; }

        [Required]
        public Guid DealerId { get; set; }

        [Required]
        public Guid MasterSlotId { get; set; }

        [Required]
        public DateTime SlotDate { get; set; }

        [Required]
        public TimeSlotStatus Status { get; set; } = TimeSlotStatus.AVAILABLE;
    }
}

