using System;
using System.Collections.Generic;

namespace EVMManagement.BLL.DTOs.Response.VehicleTimeSlot
{
    public class SlotGroupDto
    {
        public DateTime SlotDate { get; set; }
        public Guid MasterSlotId { get; set; }

        // Inventory counts
        public int TotalVehiclesInVariant { get; set; }
        public int BookedCount { get; set; }
        public int AvailableCount { get; set; }

        // Detail list of available vehicles in this slot
        public List<VehicleDetailDto>? Vehicles { get; set; }

        // Internal fields (used for calculating time display, not exposed in response if using JsonIgnore)
        [System.Text.Json.Serialization.JsonIgnore]
        public int StartOffsetMinutes { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public int DurationMinutes { get; set; }

        // Helper properties for time display
        public TimeSpan StartTime 
        { 
            get 
            { 
                return TimeSpan.FromMinutes(StartOffsetMinutes); 
            } 
        }

        public TimeSpan EndTime 
        { 
            get 
            { 
                return TimeSpan.FromMinutes(StartOffsetMinutes + DurationMinutes); 
            } 
        }

        public string TimeDisplay 
        { 
            get 
            { 
                return $"{StartTime:hh\\:mm}-{EndTime:hh\\:mm}"; 
            } 
        }
    }

    /// <summary>
    /// Grouped by date - contains all slots and vehicles for a specific date
    /// </summary>
    public class DaySlotGroupDto
    {
        public DateTime SlotDate { get; set; }

        // Inventory counts (total for this date)
        public int TotalVehiclesInVariant { get; set; }

        // All time slots available on this date, each with their own available vehicles
        public List<TimeSlotWithVehiclesDto>? TimeSlots { get; set; }
    }

    /// <summary>
    /// Time slot with vehicles available in that specific slot
    /// </summary>
    public class TimeSlotWithVehiclesDto
    {
        public Guid MasterSlotId { get; set; }
        public int BookedCount { get; set; }
        public int AvailableCount { get; set; }

        // Vehicles available for THIS specific time slot
        public List<VehicleDetailDto>? Vehicles { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public int StartOffsetMinutes { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public int DurationMinutes { get; set; }

        public TimeSpan StartTime
        {
            get
            {
                return TimeSpan.FromMinutes(StartOffsetMinutes);
            }
        }

        public TimeSpan EndTime
        {
            get
            {
                return TimeSpan.FromMinutes(StartOffsetMinutes + DurationMinutes);
            }
        }

        public string TimeDisplay
        {
            get
            {
                return $"{StartTime:hh\\:mm}-{EndTime:hh\\:mm}";
            }
        }
    }

    public class VehicleDetailDto
    {
        public Guid VehicleId { get; set; }
        public string? Vin { get; set; }
    }

    /// <summary>
    /// Summary of available slots for a specific date (ngày + danh sách slot)
    /// </summary>
    public class DaySlotsummaryDto
    {
        public DateTime SlotDate { get; set; }

        // List of available time slots on this date
        public List<SlotSummaryDto>? Slots { get; set; }
    }

    /// <summary>
    /// Summary of a single time slot
    /// </summary>
    public class SlotSummaryDto
    {
        public Guid MasterSlotId { get; set; }
        public int AvailableCount { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public int StartOffsetMinutes { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public int DurationMinutes { get; set; }

        public TimeSpan StartTime
        {
            get
            {
                return TimeSpan.FromMinutes(StartOffsetMinutes);
            }
        }

        public TimeSpan EndTime
        {
            get
            {
                return TimeSpan.FromMinutes(StartOffsetMinutes + DurationMinutes);
            }
        }

        public string TimeDisplay
        {
            get
            {
                return $"{StartTime:hh\\:mm}-{EndTime:hh\\:mm}";
            }
        }
    }

    /// <summary>
    /// Vehicles available for a specific time slot on a specific date
    /// </summary>
    public class SlotVehiclesDto
    {
        public DateTime SlotDate { get; set; }
        public Guid MasterSlotId { get; set; }
        public int AvailableCount { get; set; }

        // List of available vehicles for this specific slot
        public List<VehicleDetailDto>? Vehicles { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public int StartOffsetMinutes { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public int DurationMinutes { get; set; }

        public TimeSpan StartTime
        {
            get
            {
                return TimeSpan.FromMinutes(StartOffsetMinutes);
            }
        }

        public TimeSpan EndTime
        {
            get
            {
                return TimeSpan.FromMinutes(StartOffsetMinutes + DurationMinutes);
            }
        }

        public string TimeDisplay
        {
            get
            {
                return $"{StartTime:hh\\:mm}-{EndTime:hh\\:mm}";
            }
        }
    }
}