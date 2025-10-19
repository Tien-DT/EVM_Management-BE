using EVMManagement.DAL.Models.Enums;
using System;

namespace EVMManagement.BLL.DTOs.Response.DealerContract
{
    public class DealerContractResponseDto
    {
        public Guid Id { get; init; }
        public Guid DealerId { get; init; }
        public string ContractCode { get; init; } = string.Empty;
        public string? Terms { get; init; }
        public DealerContractStatus Status { get; init; }
        public DateTime? EffectiveDate { get; init; }
        public DateTime? ExpirationDate { get; init; }
        public DateTime? SignedAt { get; init; }
        public Guid? SignedByDealerUserId { get; init; }
        public Guid? SignedByEvmUserId { get; init; }
        public string? ContractLink { get; init; }
        public DateTime CreatedDate { get; init; }
        public DateTime? ModifiedDate { get; init; }
        public DateTime? DeletedDate { get; init; }
        public bool IsDeleted { get; init; }
    }
}
