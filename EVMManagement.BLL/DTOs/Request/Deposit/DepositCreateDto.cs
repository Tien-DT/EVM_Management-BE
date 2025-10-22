using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.Deposit
{
    public class DepositCreateDto
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        [Required]
        public DepositStatus Status { get; set; }

        public Guid? ReceivedByUserId { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
