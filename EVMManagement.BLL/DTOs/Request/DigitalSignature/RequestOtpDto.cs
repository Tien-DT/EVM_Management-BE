using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.DigitalSignature
{
    public class RequestOtpDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string SignerEmail { get; set; } = string.Empty;

        [Required]
        public SignatureEntityType EntityType { get; set; }

        public Guid? ContractId { get; set; }

        public Guid? HandoverRecordId { get; set; }
    }
}
