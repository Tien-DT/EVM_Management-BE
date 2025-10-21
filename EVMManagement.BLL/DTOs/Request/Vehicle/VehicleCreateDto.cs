using EVMManagement.DAL.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.Vehicle
{
    public class VehicleCreateDto
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

        public VehiclePurpose Purpose { get; set; } = VehiclePurpose.FOR_SALE;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }
    }
}
