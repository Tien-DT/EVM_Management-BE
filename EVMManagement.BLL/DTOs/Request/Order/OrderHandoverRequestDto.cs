using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.Order
{
    public class OrderHandoverRequestDto
    {
        public DateTime? HandoverDate { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}

