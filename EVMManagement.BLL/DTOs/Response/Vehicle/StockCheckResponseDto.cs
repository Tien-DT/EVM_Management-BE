using System;
using System.Collections.Generic;

namespace EVMManagement.BLL.DTOs.Response.Vehicle
{
    public class StockCheckResponseDto
    {
        public bool IsInStock { get; set; }

        public int AvailableQuantity { get; set; }

        public int RequestedQuantity { get; set; }

        public Guid? WarehouseId { get; set; }

        public string? WarehouseName { get; set; }

        public List<Guid> AvailableVehicleIds { get; set; } = new List<Guid>();
    }
}

