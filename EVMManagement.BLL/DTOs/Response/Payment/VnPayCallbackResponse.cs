using System;

namespace EVMManagement.BLL.DTOs.Response.Payment
{
    public class VnPayCallbackResponse
    {
        public bool Success { get; set; }
        public string ResponseCode { get; set; } = string.Empty;
        public string TransactionCode { get; set; } = string.Empty;
        public string VnpayTransactionNo { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string BankCode { get; set; } = string.Empty;
        public string CardType { get; set; } = string.Empty;
        public string OrderInfo { get; set; } = string.Empty;
        public DateTime PayDate { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? TransactionId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
