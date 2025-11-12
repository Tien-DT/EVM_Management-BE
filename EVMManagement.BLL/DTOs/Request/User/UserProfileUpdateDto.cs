using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.User
{
    public class UserProfileUpdateDto
    {
        public Guid? DealerId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FullName { get; set; } = string.Empty;

        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải gồm đúng 10 chữ số.")]
        public string? Phone { get; set; }

        [RegularExpression(@"^\d{12}$", ErrorMessage = "Căn cước phải gồm đúng 12 chữ số.")]
        public string? CardId { get; set; }

        [MaxLength(255)]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string? Email { get; set; }
    }
}
