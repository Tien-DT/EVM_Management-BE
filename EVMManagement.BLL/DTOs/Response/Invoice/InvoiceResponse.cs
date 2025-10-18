using System;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Response.Invoice
{
    public class InvoiceResponse
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string InvoiceCode { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public InvoiceStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
