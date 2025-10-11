using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Models.Entities
{
    public class InstallmentPlan : BaseEntity
    {
        [Required]
        public Guid OrderId { get; set; }

        [MaxLength(100)]
        public string? Provider { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrincipalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal InterestRate { get; set; }

        [Required]
        public int NumberOfInstallments { get; set; }

        [Required]
        public InstallmentPlanStatus Status { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
        public virtual Order Order { get; set; } = null!;
        public virtual ICollection<InstallmentPayment> InstallmentPayments { get; set; } = new HashSet<InstallmentPayment>();
    }
}
