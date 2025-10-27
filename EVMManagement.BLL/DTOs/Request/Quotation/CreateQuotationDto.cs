using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.Quotation
{
    public class CreateQuotationDto
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        public Guid? CustomerId { get; set; }

        public Guid? OrderId { get; set; }  // Link to existing Order if creating quotation for an order

        [Required]
        public Guid CreatedByUserId { get; set; }

        public string? Note { get; set; }

        public QuotationStatus Status { get; set; } = QuotationStatus.DRAFT;

        public DateTime? ValidUntil { get; set; }

        [Required]
        public List<CreateQuotationDetailDto> QuotationDetails { get; set; } = new List<CreateQuotationDetailDto>();
    }

    public class CreateQuotationDetailDto
    {
        [Required]
        public Guid VehicleVariantId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        public int DiscountPercent { get; set; } = 0;

        public string? Note { get; set; }
    }
}
