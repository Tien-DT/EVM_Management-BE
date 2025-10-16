using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.Quotation
{
    public class UpdateQuotationDto
    {
        [MaxLength(50)]
        public string? Code { get; set; }

        public Guid? CustomerId { get; set; }

        public string? Note { get; set; }

        public QuotationStatus? Status { get; set; }

        public DateTime? ValidUntil { get; set; }

        public List<UpdateQuotationDetailDto>? QuotationDetails { get; set; }
    }

    public class UpdateQuotationDetailDto
    {
        public Guid? Id { get; set; }

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
