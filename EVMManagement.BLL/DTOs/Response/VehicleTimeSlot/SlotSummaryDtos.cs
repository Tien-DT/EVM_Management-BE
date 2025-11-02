using System;
using System.Collections.Generic;

namespace EVMManagement.BLL.DTOs.Response.VehicleTimeSlot
{
    public class DaySlotsummaryDto
    {
        public DateTime SlotDate { get; set; }

        // List of available time slots on this date
        public List<SlotSummaryDto>? Slots { get; set; }
    }

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
}
