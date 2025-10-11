using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.DAL.Models.Entities
{
    public class VehicleModel : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public DateTime? LaunchDate { get; set; }

        public string? Description { get; set; }

        public bool Status { get; set; } = true;

        public int? Ranking { get; set; }
        public virtual ICollection<VehicleVariant> VehicleVariants { get; set; } = new HashSet<VehicleVariant>();
    }
}
