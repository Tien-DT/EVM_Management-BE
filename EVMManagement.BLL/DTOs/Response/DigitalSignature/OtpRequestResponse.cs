using System;

namespace EVMManagement.BLL.DTOs.Response.DigitalSignature
{
    public class OtpRequestResponse
    {
        public Guid SignatureId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
