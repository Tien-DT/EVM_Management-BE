using System;

namespace EVMManagement.BLL.DTOs.Response.OrderDetail
{
    public class OrderDetailResponse
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid VehicleVariantId { get; set; }
        public Guid? VehicleId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int DiscountPercent { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public EVMManagement.BLL.DTOs.Response.Vehicle.VehicleVariantDetailDto? VehicleVariant { get; set; }
        public EVMManagement.BLL.DTOs.Response.Vehicle.VehicleDetailResponseDto? Vehicle { get; set; }
    }
}
