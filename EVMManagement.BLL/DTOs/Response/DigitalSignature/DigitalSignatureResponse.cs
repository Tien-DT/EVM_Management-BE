using System;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Response.DigitalSignature
{
    public class DigitalSignatureResponse
    {
        public Guid Id { get; set; }
        public string SignerEmail { get; set; } = string.Empty;
        public string? SignerName { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public SignatureEntityType EntityType { get; set; }
        public Guid? ContractId { get; set; }
        public Guid? HandoverRecordId { get; set; }
        public Guid? DealerContractId { get; set; }
        public SignatureStatus Status { get; set; }
        public DateTime? SignedAt { get; set; }
        public string? FileUrl { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
