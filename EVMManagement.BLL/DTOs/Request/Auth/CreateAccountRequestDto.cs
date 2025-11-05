using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.Auth
{
    public class CreateAccountRequestDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public AccountRole Role { get; set; }

        [Required]
        [MaxLength(255)]
        public string FullName { get; set; } = string.Empty;

        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số.")]
        public string? Phone { get; set; }

        [RegularExpression(@"^\d{12}$", ErrorMessage = "Căn cước phải gồm 12 chữ số.")]
        public string? CardId { get; set; }
    }
}
