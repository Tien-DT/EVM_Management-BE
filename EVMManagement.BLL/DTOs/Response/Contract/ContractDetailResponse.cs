using System;
using EVMManagement.BLL.DTOs.Response.Customer;
using EVMManagement.BLL.DTOs.Response.Dealer;
using EVMManagement.BLL.DTOs.Response.Order;
using EVMManagement.BLL.DTOs.Response.User;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Response.Contract
{
    public class ContractDetailResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? DealerId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public Guid? SignedByUserId { get; set; }
        public string? Terms { get; set; }
        public ContractStatus Status { get; set; }
        public ContractType ContractType { get; set; }
        public DateTime? SignedAt { get; set; }
        public string? ContractLink { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public OrderResponse? Order { get; set; }
        public CustomerResponse? Customer { get; set; }
        public DealerResponseDto? Dealer { get; set; }
        public UserProfileResponse? CreatedByUser { get; set; }
        public UserProfileResponse? SignedByUser { get; set; }
    }
}
