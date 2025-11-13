using System;

namespace EVMManagement.BLL.DTOs.Response.Payment
{
    public class PaymentResponse
    {
        public string PaymentUrl { get; set; } = string.Empty;
        public string TransactionCode { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string PaymentGateway { get; set; } = string.Empty;
    }
}
