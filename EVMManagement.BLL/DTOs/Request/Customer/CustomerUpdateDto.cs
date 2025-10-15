using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.Customer
{
    public class CustomerUpdateDto
    {
        [MaxLength(255)]
        public string? FullName { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

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
    }
}
