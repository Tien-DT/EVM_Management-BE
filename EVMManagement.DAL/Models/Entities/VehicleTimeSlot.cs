using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Models.Entities
{
    public class VehicleTimeSlot : BaseEntity
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
        public TimeSlotStatus Status { get; set; }
        public virtual Vehicle Vehicle { get; set; } = null!;
        public virtual Dealer Dealer { get; set; } = null!;
        public virtual MasterTimeSlot MasterSlot { get; set; } = null!;
        public virtual TestDriveBooking? TestDriveBooking { get; set; }
    }
}
