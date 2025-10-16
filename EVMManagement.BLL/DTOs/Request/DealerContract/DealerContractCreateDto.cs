using EVMManagement.DAL.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.DealerContract
{
    public class DealerContractCreateDto
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
        public Guid? SignedByDealerUserId { get; set; }
        public Guid? SignedByEvmUserId { get; set; }
        [MaxLength(500)]
        public string? ContractLink { get; set; }
    }
}
