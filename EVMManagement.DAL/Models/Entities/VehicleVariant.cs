using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVMManagement.DAL.Models.Entities
{
    public class VehicleVariant : BaseEntity
    {
        public Guid ModelId { get; set; }

        [MaxLength(50)]
        public string? Color { get; set; }

        public int? ChargingTime { get; set; }

        [MaxLength(100)]
        public string? Engine { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Capacity { get; set; }

        [MaxLength(100)]
        public string? ShockAbsorbers { get; set; }

        [MaxLength(100)]
        public string? BatteryType { get; set; }

        [MaxLength(100)]
        public string? BatteryLife { get; set; }

        [Column(TypeName = "decimal(10,2)")]
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

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }

        public int? TrunkWidth { get; set; }

        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ChargingCapacity { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }
        public virtual VehicleModel VehicleModel { get; set; } = null!;
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new HashSet<Vehicle>();
        public virtual ICollection<QuotationDetail> QuotationDetails { get; set; } = new HashSet<QuotationDetail>();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new HashSet<OrderDetail>();
        public virtual ICollection<VehiclePromotion> VehiclePromotions { get; set; } = new HashSet<VehiclePromotion>();
    }
}
