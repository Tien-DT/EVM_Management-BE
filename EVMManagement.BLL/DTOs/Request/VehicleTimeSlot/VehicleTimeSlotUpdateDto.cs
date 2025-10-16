using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.VehicleTimeSlot
{
    public class VehicleTimeSlotUpdateDto
    {
        public Guid? VehicleId { get; set; }

        public Guid? DealerId { get; set; }

        public Guid? MasterSlotId { get; set; }

        public DateTime? SlotDate { get; set; }

        public TimeSlotStatus? Status { get; set; }
    }
}

