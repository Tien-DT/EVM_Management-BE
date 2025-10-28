using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Models.Entities
{
    public class Quotation : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        public Guid? CustomerId { get; set; }

        [Required]
        public Guid CreatedByUserId { get; set; }

        public string? Note { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal? Subtotal { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal? Tax { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal? Total { get; set; }

        [Required]
        public QuotationStatus Status { get; set; }

        public DateTime? ValidUntil { get; set; }
        public virtual Customer? Customer { get; set; }
        public virtual UserProfile CreatedByUser { get; set; } = null!;
        public virtual ICollection<QuotationDetail> QuotationDetails { get; set; } = new HashSet<QuotationDetail>();
        public virtual Order? Order { get; set; }
    }
}
