using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Models.Entities
{
    public class Contract : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public Guid OrderId { get; set; }

        public Guid? CustomerId { get; set; }

        public Guid? DealerId { get; set; }

        [Required]
        public Guid CreatedByUserId { get; set; }

        public Guid? SignedByUserId { get; set; }

        public string? Terms { get; set; }

        [Required]
        public ContractStatus Status { get; set; }

        [Required]
        public ContractType ContractType { get; set; }

        public DateTime? SignedAt { get; set; }

        [MaxLength(500)]
        public string? ContractLink { get; set; }
        public virtual Order Order { get; set; } = null!;
        public virtual Customer? Customer { get; set; }
        public virtual Dealer? Dealer { get; set; }
        public virtual UserProfile CreatedByUser { get; set; } = null!;
        public virtual UserProfile? SignedByUser { get; set; }
        public virtual ICollection<DigitalSignature> DigitalSignatures { get; set; } = new List<DigitalSignature>();
    }
}
