using System;
using System.Collections.Generic;

namespace EVMManagement.BLL.DTOs.Response.VehicleTimeSlot
{
    public class AvailableVehiclesForSlotDto
    {
        public Guid DealerId { get; set; }
        public DateTime SlotDate { get; set; }
        public Guid MasterSlotId { get; set; }
        public int TotalAvailable { get; set; }
        public int AlreadyAssigned { get; set; }
        public List<VehicleForAssignmentDto> AvailableVehicles { get; set; } = new List<VehicleForAssignmentDto>();
    }

    public class VehicleForAssignmentDto
    {
        public Guid Id { get; set; }
        public string Vin { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
       
        public string Color { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsAlreadyAssigned { get; set; }
    }
}
