using System;
using System.Collections.Generic;

namespace EVMManagement.BLL.DTOs.Response.OrderDetail
{
    public class OrderDetailBulkCreateResponse
    {
        public Guid OrderId { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? FinalAmount { get; set; }
        public List<OrderDetailResponse> CreatedOrderDetails { get; set; } = new List<OrderDetailResponse>();
    }
}
