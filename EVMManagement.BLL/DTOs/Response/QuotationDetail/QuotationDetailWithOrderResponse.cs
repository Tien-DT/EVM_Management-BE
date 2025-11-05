using System;

namespace EVMManagement.BLL.DTOs.Response.QuotationDetail
{
    public class QuotationDetailWithOrderResponse
    {
        public Guid Id { get; set; }
        public Guid QuotationId { get; set; }
        public Guid VehicleVariantId { get; set; }
        public string? VehicleVariantColor { get; set; }
        public string? VehicleModelName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int DiscountPercent { get; set; }
        public decimal LineTotal { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public OrderBasicResponse? Order { get; set; }
    }
}
