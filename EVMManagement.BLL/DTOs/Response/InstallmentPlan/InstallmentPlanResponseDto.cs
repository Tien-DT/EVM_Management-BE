using System;
using System.Collections.Generic;
using EVMManagement.BLL.DTOs.Response.InstallmentPayment;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Response.InstallmentPlan
{
    public class InstallmentPlanResponseDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid? CustomerId { get; set; }
        public string? Provider { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int NumberOfInstallments { get; set; }
        public InstallmentPlanStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public IList<InstallmentPaymentResponseDto> Payments { get; set; } = new List<InstallmentPaymentResponseDto>();
    }
}
