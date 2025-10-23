using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.Order
{
    public class CustomerNotificationRequestDto
    {
        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? EmailSubject { get; set; }
    }
}

