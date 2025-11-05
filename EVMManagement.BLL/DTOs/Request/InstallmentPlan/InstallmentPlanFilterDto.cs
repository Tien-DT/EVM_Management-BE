using System;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.InstallmentPlan
{
    public class InstallmentPlanFilterDto
    {
        public Guid? Id { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? CustomerId { get; set; }
        public InstallmentPlanStatus? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
