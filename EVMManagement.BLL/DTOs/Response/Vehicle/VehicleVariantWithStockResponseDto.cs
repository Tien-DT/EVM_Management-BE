using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVMManagement.BLL.DTOs.Response.Vehicle
{
    public class VehicleVariantWithStockResponseDto
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
        public int AvailableStock { get; init; }
        public DateTime CreatedDate { get; init; }
        public DateTime? ModifiedDate { get; init; }
    }
}
