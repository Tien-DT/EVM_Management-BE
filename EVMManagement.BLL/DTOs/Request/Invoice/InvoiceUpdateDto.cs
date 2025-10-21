using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.Invoice
{
    public class InvoiceUpdateDto
    {
        public Guid? OrderId { get; set; }

        [MaxLength(50)]
        public string? InvoiceCode { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? TotalAmount { get; set; }

        public InvoiceStatus? Status { get; set; }
    }
}
