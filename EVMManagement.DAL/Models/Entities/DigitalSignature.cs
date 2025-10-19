using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Models.Entities
{
    public class DigitalSignature : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string SignerEmail { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? SignerName { get; set; }

        public string? SignatureData { get; set; }

        [MaxLength(100)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        [Required]
        public SignatureEntityType EntityType { get; set; }

        public Guid? ContractId { get; set; }

        public Guid? HandoverRecordId { get; set; }

        public Guid? DealerContractId { get; set; }

        [Required]
        public SignatureStatus Status { get; set; } = SignatureStatus.OTP_SENT;

        [MaxLength(255)]
        public string? OtpCode { get; set; }

        public DateTime? OtpExpiresAt { get; set; }

        public int OtpAttemptCount { get; set; } = 0;

        public DateTime? SignedAt { get; set; }

        [MaxLength(255)]
        public string? VerificationCode { get; set; }

        [MaxLength(500)]
        public string? FileUrl { get; set; }

        public string? Notes { get; set; }

        public virtual Contract? Contract { get; set; }
        public virtual HandoverRecord? HandoverRecord { get; set; }
        public virtual DealerContract? DealerContract { get; set; }
    }
}
