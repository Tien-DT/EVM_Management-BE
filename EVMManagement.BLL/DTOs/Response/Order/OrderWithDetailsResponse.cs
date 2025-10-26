using System.Collections.Generic;
using EVMManagement.BLL.DTOs.Response.OrderDetail;

namespace EVMManagement.BLL.DTOs.Response.Order
{
    public class OrderWithDetailsResponse : OrderResponse
    {
        public IList<OrderDetailResponse> OrderDetails { get; set; } = new List<OrderDetailResponse>();
    }
}
