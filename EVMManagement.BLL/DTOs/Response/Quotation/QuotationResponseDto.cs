using System;
using System.Collections.Generic;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Response.Quotation
{
    public class QuotationResponseDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public Guid? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
        public string? Note { get; set; }
        public decimal? Subtotal { get; set; }
        public decimal? Tax { get; set; }
        public decimal? Total { get; set; }
        public QuotationStatus Status { get; set; }
        public DateTime? ValidUntil { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public List<QuotationDetailResponseDto> QuotationDetails { get; set; } = new List<QuotationDetailResponseDto>();
    }

    public class QuotationDetailResponseDto
    {
        public Guid Id { get; set; }
        public Guid QuotationId { get; set; }
        public Guid VehicleVariantId { get; set; }
        public string? VehicleVariantColor { get; set; }
        public string? VehicleModelName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int DiscountPercent { get; set; }
        public decimal LineTotal { get; set; }
        public string? Note { get; set; }
    }
}
