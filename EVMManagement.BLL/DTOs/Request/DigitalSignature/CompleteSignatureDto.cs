using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.DigitalSignature
{
    public class CompleteSignatureDto
    {
        [Required]
        public Guid SignatureId { get; set; }

        [Required]
        [MaxLength(255)]
        public string SignerName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? FileUrl { get; set; }

        public string? Notes { get; set; }
    }
}
