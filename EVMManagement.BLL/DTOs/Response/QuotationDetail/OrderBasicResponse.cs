using System;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Response.QuotationDetail
{
    public class OrderBasicResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public Guid? QuotationId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? DealerId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public OrderStatus Status { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? FinalAmount { get; set; }
        public DateTime? ExpectedDeliveryAt { get; set; }
        public OrderType OrderType { get; set; }
        public bool IsFinanced { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
