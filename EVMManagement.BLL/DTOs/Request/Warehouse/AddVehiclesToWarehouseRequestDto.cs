using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.Warehouse
{
    public class AddVehiclesToWarehouseRequestDto
    {
        [Required]
        public Guid VehicleVariantId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one VIN number is required")]
        public List<string> VinNumbers { get; set; } = new List<string>();

        public VehiclePurpose Purpose { get; set; } = VehiclePurpose.FOR_SALE;
    }
}

