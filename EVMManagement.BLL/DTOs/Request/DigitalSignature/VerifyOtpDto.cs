using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.DigitalSignature
{
    public class VerifyOtpDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string SignerEmail { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string OtpCode { get; set; } = string.Empty;

        [Required]
        public SignatureEntityType EntityType { get; set; }

        public Guid? ContractId { get; set; }

        public Guid? HandoverRecordId { get; set; }
    }
}
