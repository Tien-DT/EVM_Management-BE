using System;

namespace EVMManagement.BLL.DTOs.Request.Payment
{
    public class PaymentRequest
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; } = string.Empty;
        public bool IsDeposit { get; set; }
        public string? BankCode { get; set; }
        public string? Locale { get; set; }
    }
}
