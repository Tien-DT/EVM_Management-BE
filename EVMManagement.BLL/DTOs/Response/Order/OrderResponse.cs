using System;
using System.Collections.Generic;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.BLL.DTOs.Response.Customer;
using EVMManagement.BLL.DTOs.Response.Dealer;
using EVMManagement.BLL.DTOs.Response.User;
using EVMManagement.BLL.DTOs.Response.Quotation;
using EVMManagement.BLL.DTOs.Response.Deposit;
using EVMManagement.BLL.DTOs.Response.OrderDetail;

namespace EVMManagement.BLL.DTOs.Response.Order
{
    public class OrderResponse
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
        public QuotationResponseDto? Quotation { get; set; }
        public CustomerResponse? Customer { get; set; }
        public DealerResponseDto? Dealer { get; set; }
        public UserProfileResponse? CreatedByUser { get; set; }
        public List<DepositResponse>? Deposits { get; set; }
        public List<OrderDetailResponse>? OrderDetails { get; set; }
    }
}
