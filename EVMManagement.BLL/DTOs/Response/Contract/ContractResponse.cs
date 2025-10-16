using System;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Response.Contract
{
    public class ContractResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string? Terms { get; set; }
        public ContractStatus Status { get; set; }
        public DateTime? SignedAt { get; set; }
        public string? ContractLink { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
