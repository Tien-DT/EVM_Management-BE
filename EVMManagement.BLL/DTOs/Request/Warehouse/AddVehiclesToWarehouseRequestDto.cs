using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.Warehouse
{
    public class AddVehiclesToWarehouseRequestDto
    {
        public Guid? WarehouseId { get; set; }

        public Guid? DealerId { get; set; }

        [Required(ErrorMessage = "VehicleVariantId là bắt buộc")]
        public Guid VehicleVariantId { get; set; }

        [Required(ErrorMessage = "Danh sách VIN là bắt buộc")]
        [MinLength(1, ErrorMessage = "Cần ít nhất một số VIN")]
        public List<string> VinNumbers { get; set; } = new List<string>();

        public VehiclePurpose Purpose { get; set; } = VehiclePurpose.FOR_SALE;
    }
}

