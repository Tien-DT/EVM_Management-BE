using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.Auth
{
    public class ChangePasswordRequestDto
    {
        [Required]
        public string OldPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
