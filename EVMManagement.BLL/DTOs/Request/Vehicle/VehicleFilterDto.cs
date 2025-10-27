using EVMManagement.DAL.Models.Enums;
using System;

namespace EVMManagement.BLL.DTOs.Request.Vehicle
{
    public class VehicleFilterDto
    {
        public VehicleStatus? Status { get; set; }
        public VehiclePurpose? Purpose { get; set; }
        public Guid? DealerId { get; set; }
        public Guid? WarehouseId { get; set; }
        public Guid? VariantId { get; set; }
        public string? Q { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
