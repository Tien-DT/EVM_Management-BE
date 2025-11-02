using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.Contract
{
    public class ContractUpdateDto
    {
        [MaxLength(50)]
        public string? Code { get; set; }

        public Guid? OrderId { get; set; }

        public Guid? CustomerId { get; set; }

        public Guid? DealerId { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public Guid? SignedByUserId { get; set; }

        public string? Terms { get; set; }

        public ContractStatus? Status { get; set; }

        public ContractType? ContractType { get; set; }

        public DateTime? SignedAt { get; set; }

        [MaxLength(500)]
        public string? ContractLink { get; set; }
    }
}
