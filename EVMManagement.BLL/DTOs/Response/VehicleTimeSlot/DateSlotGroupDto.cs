using System;
using System.Collections.Generic;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Response.VehicleTimeSlot
{
    public class DateSlotGroupDto
    {
        public string Date { get; set; } = string.Empty; 
        public int TotalSlots { get; set; }
        public int TotalVehiclesAvailable { get; set; }
        public int TotalVehiclesBooked { get; set; }
        public List<MasterSlotWithVehiclesDto> MasterSlots { get; set; } = new List<MasterSlotWithVehiclesDto>();
    }

    public class MasterSlotWithVehiclesDto
    {
        public Guid MasterSlotId { get; set; }
        public string MasterSlotCode { get; set; } = string.Empty;
        public int StartOffsetMinutes { get; set; }
        public int DurationMinutes { get; set; }
        public string StartTime { get; set; } = string.Empty; 
        public string EndTime { get; set; } = string.Empty;   
        public bool IsActive { get; set; }
        public int TotalVehicles { get; set; }
        public int AvailableVehicles { get; set; }
        public int BookedVehicles { get; set; }
        public List<VehicleTimeSlotDetailDto> Vehicles { get; set; } = new List<VehicleTimeSlotDetailDto>();
    }

    public class VehicleTimeSlotDetailDto
    {
        public Guid VehicleTimeSlotId { get; set; }
        public TimeSlotStatus Status { get; set; }
        public VehicleSummaryDetailDto Vehicle { get; set; } = new VehicleSummaryDetailDto();
    }

    public class VehicleSummaryDetailDto
    {
        public Guid Id { get; set; }
        public string Vin { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public VehicleStatus Status { get; set; }
        public VehiclePurpose Purpose { get; set; }
        public string? Color { get; set; }
    }
}
