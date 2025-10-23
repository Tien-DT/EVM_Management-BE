using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.Order
{
    public class DealerManagerApprovalRequestDto
    {
        [Required]
        public Guid RequestedByUserId { get; set; }

        [MaxLength(1000)]
        public string? Note { get; set; }
    }
}

