using System;

namespace EVMManagement.BLL.DTOs.Response.Customer
{
    public class CustomerResponse
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public DateTime? Dob { get; set; }
        public string? CardId { get; set; }
        public Guid? DealerId { get; set; }
        public Guid? ManagedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
