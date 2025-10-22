using System;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Response.Deposit
{
    public class DepositResponse
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public DepositStatus Status { get; set; }
        public Guid? ReceivedByUserId { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
