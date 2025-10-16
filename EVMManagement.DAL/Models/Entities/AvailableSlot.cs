using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.DAL.Models.Entities
{
    public class AvailableSlot : BaseEntity
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

        public virtual Vehicle Vehicle { get; set; } = null!;
        public virtual Dealer Dealer { get; set; } = null!;
        public virtual MasterTimeSlot MasterSlot { get; set; } = null!;
    }
}

