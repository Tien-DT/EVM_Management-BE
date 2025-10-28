using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EVMManagement.BLL.DTOs.Request.Vehicle;

namespace EVMManagement.BLL.DTOs.Request.Warehouse
{
    public class AddVehiclesToEvmWarehouseDto
    {
        [Required(ErrorMessage = "WarehouseId là bắt buộc")]
        public Guid WarehouseId { get; set; }

        [Required(ErrorMessage = "Danh sách xe là bắt buộc")]
        [MinLength(1, ErrorMessage = "Cần ít nhất một xe")]
        public List<VehicleBulkCreateDto> Vehicles { get; set; } = new List<VehicleBulkCreateDto>();
    }
}
