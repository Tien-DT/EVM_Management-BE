using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Models.Entities
{
    public class Deposit : BaseEntity
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        [Required]
        public DepositStatus Status { get; set; }

        public Guid? ReceivedByUserId { get; set; }

        public string? Note { get; set; }

        public virtual Order Order { get; set; } = null!;
        public virtual Account? ReceivedByUser { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; } = new HashSet<Transaction>();
    }
}
