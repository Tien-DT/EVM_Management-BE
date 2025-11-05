using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.InstallmentPayment
{
    public class InstallmentPaymentCreateDto
    {
        [Required]
        public Guid PlanId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int InstallmentNumber { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal AmountDue { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public InstallmentPaymentStatus Status { get; set; } = InstallmentPaymentStatus.PENDING;
    }
}
