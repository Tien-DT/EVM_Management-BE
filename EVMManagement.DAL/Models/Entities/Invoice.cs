using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Models.Entities
{
    public class Invoice : BaseEntity
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        [MaxLength(50)]
        public string InvoiceCode { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public InvoiceStatus Status { get; set; }
        public virtual Order Order { get; set; } = null!;
        public virtual ICollection<Transaction> Transactions { get; set; } = new HashSet<Transaction>();
    }
}
