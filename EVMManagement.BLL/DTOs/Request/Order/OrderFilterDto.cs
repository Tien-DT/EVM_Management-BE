using EVMManagement.DAL.Models.Enums;
using System;

namespace EVMManagement.BLL.DTOs.Request.Order
{
    public class OrderFilterDto
    {
        public string? Code { get; set; }
        public Guid? QuotationId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? DealerId { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public OrderType? OrderType { get; set; }
        public OrderStatus? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
