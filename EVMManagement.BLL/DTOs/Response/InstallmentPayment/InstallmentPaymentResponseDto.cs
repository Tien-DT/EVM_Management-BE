using System;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Response.InstallmentPayment
{
    public class InstallmentPaymentResponseDto
    {
        public Guid Id { get; set; }
        public Guid PlanId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? CustomerId { get; set; }
        public int InstallmentNumber { get; set; }
        public decimal AmountDue { get; set; }
        public DateTime DueDate { get; set; }
        public InstallmentPaymentStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
