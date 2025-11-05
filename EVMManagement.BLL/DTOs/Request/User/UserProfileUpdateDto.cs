using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.User
{
    public class UserProfileUpdateDto
    {
        
        public Guid? DealerId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(10)]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Phone must start with 0 and be exactly 10 digits.")]
        public string? Phone { get; set; }

        [MaxLength(12)]
        public string? CardId { get; set; }

        [MaxLength(255)]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }
    }
}
