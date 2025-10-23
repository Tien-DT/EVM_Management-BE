using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.Order
{
    public class CustomerConfirmationRequestDto
    {
        [Required]
        public bool IsConfirmed { get; set; }

        [MaxLength(1000)]
        public string? CustomerNote { get; set; }
    }
}

