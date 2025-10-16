using System;

namespace EVMManagement.BLL.DTOs.Response.QuotationDetail
{
    public class QuotationDetailResponse
    {
        public Guid Id { get; set; }
        public Guid QuotationId { get; set; }
        public Guid VehicleVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int DiscountPercent { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
