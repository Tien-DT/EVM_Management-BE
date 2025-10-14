using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.Auth
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
