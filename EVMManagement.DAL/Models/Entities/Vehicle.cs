using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Models.Entities
{
    public class Vehicle : BaseEntity
    {
        [Required]
        public Guid VariantId { get; set; }

        [Required]
        public Guid WarehouseId { get; set; }

        [Required]
        [MaxLength(17)]
        public string Vin { get; set; } = string.Empty;

        [Required]
        public VehicleStatus Status { get; set; }

        [Required]
        public VehiclePurpose Purpose { get; set; } = VehiclePurpose.FOR_SALE;
        public virtual VehicleVariant VehicleVariant { get; set; } = null!;
        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new HashSet<OrderDetail>();
        public virtual TransportDetail? TransportDetail { get; set; }
        public virtual HandoverRecord? HandoverRecord { get; set; }
        public virtual ICollection<VehicleTimeSlot> VehicleTimeSlots { get; set; } = new HashSet<VehicleTimeSlot>();
    }
}
