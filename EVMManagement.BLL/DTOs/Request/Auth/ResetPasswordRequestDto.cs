using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.Auth
{
    public class ResetPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string ResetToken { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
