using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.Customer
{
    public class CustomerCreateDto
    {
        [MaxLength(255)]
        public string? FullName { get; set; }

        [Required]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(255)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public DateTime? Dob { get; set; }

        [MaxLength(50)]
        public string? CardId { get; set; }

        public Guid? DealerId { get; set; }
    }
}
