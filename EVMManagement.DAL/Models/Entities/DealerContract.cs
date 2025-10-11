using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Models.Entities
{
    public class DealerContract : BaseEntity
    {
        [Required]
        public Guid DealerId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ContractCode { get; set; } = string.Empty;

        public string? Terms { get; set; }

        [Required]
        public DealerContractStatus Status { get; set; }

        public DateTime? EffectiveDate { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public DateTime? SignedAt { get; set; }

        public Guid? SignedByDealerUserId { get; set; }

        public Guid? SignedByEvmUserId { get; set; }

        [MaxLength(500)]
        public string? ContractLink { get; set; }

        public virtual Dealer Dealer { get; set; } = null!;
        public virtual UserProfile? SignedByDealerUser { get; set; }
        public virtual UserProfile? SignedByEvmUser { get; set; }
    }
}
