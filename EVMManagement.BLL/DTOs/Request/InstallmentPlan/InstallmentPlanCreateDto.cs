using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.InstallmentPlan
{
    public class InstallmentPlanCreateDto
    {
        [Required]
        public Guid OrderId { get; set; }

        [MaxLength(100)]
        public string? Provider { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal PrincipalAmount { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal InterestRate { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int NumberOfInstallments { get; set; }

        public InstallmentPlanStatus Status { get; set; } = InstallmentPlanStatus.ACTIVE;

        [Required]
        public DateTime StartDate { get; set; }
    }
}
