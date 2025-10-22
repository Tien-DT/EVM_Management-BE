using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Models.Entities
{
    public class Order : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        public Guid? QuotationId { get; set; }

        public Guid? CustomerId { get; set; }

        public Guid? DealerId { get; set; }

        [Required]
        public Guid CreatedByUserId { get; set; }

        [Required]
        public OrderStatus Status { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? FinalAmount { get; set; }

        public DateTime? ExpectedDeliveryAt { get; set; }

        [Required]
        public OrderType OrderType { get; set; }

        public bool IsFinanced { get; set; } = false;
        public virtual Quotation? Quotation { get; set; }
        public virtual Customer? Customer { get; set; }
        public virtual Dealer? Dealer { get; set; }
        public virtual UserProfile CreatedByUser { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new HashSet<OrderDetail>();
        public virtual Contract? Contract { get; set; }
        public virtual ICollection<Deposit> Deposits { get; set; } = new HashSet<Deposit>();
        public virtual Invoice? Invoice { get; set; }
        public virtual InstallmentPlan? InstallmentPlan { get; set; }
        public virtual HandoverRecord? HandoverRecord { get; set; }
        public virtual ICollection<Report> Reports { get; set; } = new HashSet<Report>();
    }
}
