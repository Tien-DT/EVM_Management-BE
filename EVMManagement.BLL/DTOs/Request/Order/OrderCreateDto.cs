using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.Order
{
    public class OrderCreateDto
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        public Guid? QuotationId { get; set; }

        public Guid? CustomerId { get; set; }

        public Guid? DealerId { get; set; }

        [Required]
        public Guid CreatedByUserId { get; set; }

        [Required]
        public OrderStatus Status { get; set; }

        public decimal? TotalAmount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal? FinalAmount { get; set; }

        public DateTime? ExpectedDeliveryAt { get; set; }

        [Required]
        public OrderType OrderType { get; set; }

        public bool IsFinanced { get; set; } = false;
    }
}
