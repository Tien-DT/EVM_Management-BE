using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.DAL.Models.Entities
{
    public class VehiclePromotion
    {
        [Required]
        public Guid VariantId { get; set; }

        [Required]
        public Guid PromotionId { get; set; }

        [MaxLength(255)]
        public string? Note { get; set; }
        public virtual VehicleVariant VehicleVariant { get; set; } = null!;
        public virtual Promotion Promotion { get; set; } = null!;
    }
}
