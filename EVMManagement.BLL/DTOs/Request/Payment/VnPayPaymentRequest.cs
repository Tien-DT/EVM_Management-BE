using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.Payment
{
    public class VnPayPaymentRequest
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(255)]
        public string OrderInfo { get; set; } = string.Empty;

        public bool IsDeposit { get; set; } = false;

        public string? BankCode { get; set; }

        public string? Locale { get; set; } = "vn";
    }
}
