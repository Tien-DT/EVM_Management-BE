using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.OrderDetail
{
    public class OrderDetailUpdateDto
    {
        public Guid? OrderId { get; set; }

        public Guid? VehicleVariantId { get; set; }

        public Guid? VehicleId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int? Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "UnitPrice must be greater than or equal to 0")]
        public decimal? UnitPrice { get; set; }

        [Range(0, 100, ErrorMessage = "DiscountPercent must be between 0 and 100")]
        public int? DiscountPercent { get; set; }

        public string? Note { get; set; }
    }
}
