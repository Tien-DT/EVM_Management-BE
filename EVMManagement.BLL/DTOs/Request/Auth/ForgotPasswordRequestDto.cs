using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.Auth
{
    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
    }
}
