using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.DAL.Models.Entities
{
    public class MasterTimeSlot
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        public int? StartOffsetMinutes { get; set; }

        public int? DurationMinutes { get; set; }

        public bool IsActive { get; set; } = true;
        public virtual ICollection<VehicleTimeSlot> VehicleTimeSlots { get; set; } = new HashSet<VehicleTimeSlot>();
    }
}
