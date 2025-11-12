using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.User
{
    public class UserProfilePatchDto
    {
        public Guid? DealerId { get; set; }

        [MaxLength(255)]
        public string? FullName { get; set; }

        [RegularExpression(@"^\d{10}$", ErrorMessage = "So dien thoai phai gom dung 10 chu so.")]
        public string? Phone { get; set; }

        [RegularExpression(@"^\d{12}$", ErrorMessage = "Can cuoc phai gom dung 12 chu so.")]
        public string? CardId { get; set; }

        [MaxLength(255)]
        [EmailAddress(ErrorMessage = "Email khong dung dinh dang.")]
        public string? Email { get; set; }
    }
}
