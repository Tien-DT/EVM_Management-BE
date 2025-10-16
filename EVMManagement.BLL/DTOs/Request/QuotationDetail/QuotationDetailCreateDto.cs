using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.QuotationDetail
{
    public class QuotationDetailCreateDto
    {
        [Required]
        public Guid QuotationId { get; set; }

        [Required]
        public Guid VehicleVariantId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "UnitPrice must be greater than or equal to 0")]
        public decimal UnitPrice { get; set; }

        [Range(0, 100, ErrorMessage = "DiscountPercent must be between 0 and 100")]
        public int DiscountPercent { get; set; } = 0;

        public string? Note { get; set; }
    }
}
