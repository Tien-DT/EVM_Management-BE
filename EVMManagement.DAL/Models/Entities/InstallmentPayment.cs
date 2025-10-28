using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Models.Entities
{
    public class InstallmentPayment : BaseEntity
    {
        [Required]
        public Guid PlanId { get; set; }

        [Required]
        public int InstallmentNumber { get; set; }

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal AmountDue { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public InstallmentPaymentStatus Status { get; set; }
        public virtual InstallmentPlan InstallmentPlan { get; set; } = null!;
        public virtual ICollection<Transaction> Transactions { get; set; } = new HashSet<Transaction>();
    }
}
