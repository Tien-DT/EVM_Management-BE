using System;

namespace EVMManagement.BLL.DTOs.Response.Payment
{
    public class PaymentCallbackResponse
    {
        public bool Success { get; set; }
        public string ResponseCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string TransactionCode { get; set; } = string.Empty;
        public string GatewayTransactionNo { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? BankCode { get; set; }
        public string? CardType { get; set; }
        public string OrderInfo { get; set; } = string.Empty;
        public DateTime PayDate { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? TransactionId { get; set; }
        public string PaymentGateway { get; set; } = string.Empty;
    }
}
