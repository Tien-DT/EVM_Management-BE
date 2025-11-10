using System;
using System.Collections.Generic;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Response.Warehouse
{
    public class DealerDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    public class VehicleModelDto
    {
        public Guid Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public DateTime? LaunchDate { get; init; }
        public string? Description { get; init; }
        public string? ImageUrl { get; init; }
        public bool Status { get; init; }
        public VehicleModelRanking? Ranking { get; init; }
        public DateTime CreatedDate { get; init; }
        public DateTime? ModifiedDate { get; init; }
    }

    public class VehicleVariantDto
    {
        public Guid Id { get; init; }
        public Guid ModelId { get; init; }
        public string? Color { get; init; }
        public int? ChargingTime { get; init; }
        public string? Engine { get; init; }
        public decimal? Capacity { get; init; }
        public string? ShockAbsorbers { get; init; }
        public string? BatteryType { get; init; }
        public string? BatteryLife { get; init; }
        public decimal? MaximumSpeed { get; init; }
        public string? DistancePerCharge { get; init; }
        public int? Weight { get; init; }
        public int? GroundClearance { get; init; }
        public string? Brakes { get; init; }
        public int? Length { get; init; }
        public int? Width { get; init; }
        public int? Height { get; init; }
        public decimal? Price { get; init; }
        public int? TrunkWidth { get; init; }
        public string? Description { get; init; }
        public decimal? ChargingCapacity { get; init; }
        public string? ImageUrl { get; init; }
        public DateTime CreatedDate { get; init; }
        public DateTime? ModifiedDate { get; init; }
        public VehicleModelDto? VehicleModel { get; init; }
    }

    public class VehicleDto
    {
        public Guid Id { get; init; }
        public Guid VariantId { get; init; }
        public string Vin { get; init; } = string.Empty;
        public VehicleStatus Status { get; init; }
        public VehiclePurpose Purpose { get; init; }
        public DateTime CreatedDate { get; init; }
        public DateTime? ModifiedDate { get; init; }
        public string? ImageUrl { get; init; }
        public VehicleVariantDto? Variant { get; init; }
    }

    public class WarehouseResponseDto
    {
        public Guid Id { get; init; }
        public Guid? DealerId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Address { get; init; }
        public int? Capacity { get; init; }
        public WarehouseType Type { get; init; }
        public DealerDto? Dealer { get; init; }
        public List<VehicleDto>? Vehicles { get; init; }
        public DateTime CreatedDate { get; init; }
        public DateTime? ModifiedDate { get; init; }
        public DateTime? DeletedDate { get; init; }
        public bool IsDeleted { get; init; }
    }
}