using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.DealerContract
{
    public class DealerOtpVerifyRequestDto
    {
        [Required]
        public string Otp { get; set; } = string.Empty;
    }
}
