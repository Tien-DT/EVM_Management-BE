using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.InstallmentPayment
{
    public class InstallmentPaymentUpdateDto
    {
        public Guid? PlanId { get; set; }

        [Range(1, int.MaxValue)]
        public int? InstallmentNumber { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? AmountDue { get; set; }

        public DateTime? DueDate { get; set; }

        public InstallmentPaymentStatus? Status { get; set; }
    }
}
