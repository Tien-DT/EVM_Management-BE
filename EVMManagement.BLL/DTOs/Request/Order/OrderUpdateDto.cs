using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.Order
{
    public class OrderUpdateDto
    {
        [MaxLength(50)]
        public string? Code { get; set; }

        public Guid? QuotationId { get; set; }

        public Guid? CustomerId { get; set; }

        public Guid? DealerId { get; set; }

        public OrderStatus? Status { get; set; }

        public decimal? TotalAmount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal? FinalAmount { get; set; }

        public DateTime? ExpectedDeliveryAt { get; set; }

        public OrderType? OrderType { get; set; }

        public bool? IsFinanced { get; set; }
    }
}
