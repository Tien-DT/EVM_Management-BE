using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models;

namespace EVMManagement.DAL.Models.Entities
{
    public class MasterTimeSlot : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        public int? StartOffsetMinutes { get; set; }

        public int? DurationMinutes { get; set; }

        public bool IsActive { get; set; } = true;
        public virtual ICollection<VehicleTimeSlot> VehicleTimeSlots { get; set; } = new HashSet<VehicleTimeSlot>();
        public Guid? DealerId { get; set; }
        public virtual Dealer? Dealer { get; set; }
    }
}
