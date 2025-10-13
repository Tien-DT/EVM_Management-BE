using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.Vehicle
{
    public class VehicleVariantCreateDto
    {
        [Required]
        public Guid ModelId { get; set; }

        [MaxLength(50)]
        public string? Color { get; set; }

        public int? ChargingTime { get; set; }

        [MaxLength(100)]
        public string? Engine { get; set; }

        public decimal? Capacity { get; set; }

        [MaxLength(100)]
        public string? ShockAbsorbers { get; set; }

        [MaxLength(100)]
        public string? BatteryType { get; set; }

        [MaxLength(100)]
        public string? BatteryLife { get; set; }

        public decimal? MaximumSpeed { get; set; }

        [MaxLength(50)]
        public string? DistancePerCharge { get; set; }

        public int? Weight { get; set; }

        public int? GroundClearance { get; set; }

        [MaxLength(100)]
        public string? Brakes { get; set; }

        public int? Length { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public decimal? Price { get; set; }

        public int? TrunkWidth { get; set; }

        public string? Description { get; set; }

        public decimal? ChargingCapacity { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }
    }
}
