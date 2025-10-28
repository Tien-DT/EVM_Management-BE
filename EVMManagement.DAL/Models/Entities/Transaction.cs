using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Models.Entities
{
    public class Transaction : BaseEntity
    {
        public Guid? InvoiceId { get; set; }

        public Guid? DepositId { get; set; }

        public Guid? InstallmentPaymentId { get; set; }

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = "VND";

        [Required]
        public TransactionStatus Status { get; set; }

        [Required]
        public DateTime TransactionTime { get; set; }

        [MaxLength(50)]
        public string? PaymentGateway { get; set; }

        [MaxLength(100)]
        public string? VnpayTransactionCode { get; set; }

        [MaxLength(100)]
        public string? VnpayTransactionNo { get; set; }

        [MaxLength(50)]
        public string? BankCode { get; set; }

        [MaxLength(50)]
        public string? CardType { get; set; }

        [MaxLength(10)]
        public string? ResponseCode { get; set; }

        [MaxLength(500)]
        public string? TransactionInfo { get; set; }

        [MaxLength(256)]
        public string? SecureHash { get; set; }
        
        public virtual Invoice? Invoice { get; set; }
        public virtual Deposit? Deposit { get; set; }
        public virtual InstallmentPayment? InstallmentPayment { get; set; }
    }
}
