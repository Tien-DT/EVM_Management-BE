using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVMManagement.DAL.Models.Entities
{
    public class OrderDetail : BaseEntity
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid VehicleVariantId { get; set; }

        public Guid? VehicleId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal UnitPrice { get; set; }

        public int DiscountPercent { get; set; } = 0;

        public string? Note { get; set; }
        public virtual Order Order { get; set; } = null!;
        public virtual VehicleVariant VehicleVariant { get; set; } = null!;
        public virtual Vehicle? Vehicle { get; set; }
    }
}
