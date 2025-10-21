using EVMManagement.DAL.Models.Enums;
using System;

namespace EVMManagement.BLL.DTOs.Response.Vehicle
{
    public class VehicleResponseDto
    {
        public Guid Id { get; init; }
        public Guid VariantId { get; init; }
        public Guid WarehouseId { get; init; }
        public string Vin { get; init; } = string.Empty;
        public string? ImageUrl { get; init; }
        public VehicleStatus Status { get; init; }
        public VehiclePurpose Purpose { get; init; }
        public DateTime CreatedDate { get; init; }
        public DateTime? ModifiedDate { get; init; }
        public DateTime? DeletedDate { get; init; }
        public bool IsDeleted { get; init; }
    }
}
