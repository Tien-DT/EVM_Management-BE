using System;

namespace EVMManagement.BLL.DTOs.Response.Vehicle
{
    public class VehicleVariantResponse
    {
        public Guid Id { get; set; }
        public Guid ModelId { get; set; }
        public string? ModelName { get; set; }
        public string? Color { get; set; }
        public int? ChargingTime { get; set; }
        public string? Engine { get; set; }
        public decimal? Capacity { get; set; }
        public string? ShockAbsorbers { get; set; }
        public string? BatteryType { get; set; }
        public string? BatteryLife { get; set; }
        public decimal? MaximumSpeed { get; set; }
        public string? DistancePerCharge { get; set; }
        public int? Weight { get; set; }
        public int? GroundClearance { get; set; }
        public string? Brakes { get; set; }
        public int? Length { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public decimal? Price { get; set; }
        public int? TrunkWidth { get; set; }
        public string? Description { get; set; }
        public decimal? ChargingCapacity { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
