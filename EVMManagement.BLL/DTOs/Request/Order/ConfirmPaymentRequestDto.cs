using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.Order
{
    public class ConfirmPaymentRequestDto
    {
        [Required]
        public PaymentMethod Method { get; set; }

        [MaxLength(100)]
        public string? TransactionReference { get; set; }

        [MaxLength(1000)]
        public string? Note { get; set; }
    }
}

