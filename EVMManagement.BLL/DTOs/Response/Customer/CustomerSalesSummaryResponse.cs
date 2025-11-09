using System;

namespace EVMManagement.BLL.DTOs.Response.Customer
{
    public class CustomerSalesSummaryResponse
    {
        public Guid ManagedBy { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
