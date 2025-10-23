using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.Deposit
{
    public class PreOrderDepositRequestDto
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        [MaxLength(500)]
        public string? BillImageUrl { get; set; }

        [MaxLength(1000)]
        public string? Note { get; set; }
    }
}

