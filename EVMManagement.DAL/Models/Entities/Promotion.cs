using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.DAL.Models.Entities
{
    public class Promotion : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public int? DiscountPercent { get; set; }

        public DateTime? StartAt { get; set; }

        public DateTime? EndAt { get; set; }

        public bool IsActive { get; set; } = true;
        public virtual ICollection<VehiclePromotion> VehiclePromotions { get; set; } = new HashSet<VehiclePromotion>();
    }
}
