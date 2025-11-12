using System;
using EVMManagement.BLL.DTOs.Response.Vehicle;

namespace EVMManagement.BLL.DTOs.Response.Promotion
{
    public class VehiclePromotionResponseDto
    {
        public Guid VariantId { get; set; }
        public Guid PromotionId { get; set; }
        public string? Note { get; set; }
        public VehicleVariantDetailDto? VehicleVariant { get; set; }
        public PromotionResponseDto? Promotion { get; set; }
    }
}

