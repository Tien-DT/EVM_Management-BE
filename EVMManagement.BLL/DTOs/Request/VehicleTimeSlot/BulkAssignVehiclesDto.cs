using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.VehicleTimeSlot
{
    public class BulkAssignVehiclesDto
    {
        [Required]
        public Guid MasterSlotId { get; set; }

        [Required]
        public DateTime SlotDate { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one vehicle must be selected")]
        public List<Guid> VehicleIds { get; set; } = new();

        public TimeSlotStatus Status { get; set; } = TimeSlotStatus.AVAILABLE;
    }
}
