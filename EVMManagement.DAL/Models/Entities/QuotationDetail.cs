using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVMManagement.DAL.Models.Entities
{
    public class QuotationDetail : BaseEntity
    {
        [Required]
        public Guid QuotationId { get; set; }

        [Required]
        public Guid VehicleVariantId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public int DiscountPercent { get; set; } = 0;

        public string? Note { get; set; }
        public virtual Quotation Quotation { get; set; } = null!;
        public virtual VehicleVariant VehicleVariant { get; set; } = null!;
    }
}
