using EVMManagement.DAL.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.Vehicle
{
    public class VehicleUpdateDto
    {
        public Guid? VariantId { get; set; }

        public Guid? WarehouseId { get; set; }

        [MaxLength(17)]
        public string? Vin { get; set; }

        public VehicleStatus? Status { get; set; }

        public VehiclePurpose? Purpose { get; set; }
    }
}
