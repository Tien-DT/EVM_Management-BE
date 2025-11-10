using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.Helpers
{

    public class BusinessHoursValidation : ValidationAttribute
    {
        private const int MORNING_START = 450; // 7:30
        private const int MORNING_END = 690;   // 11:30
        private const int AFTERNOON_START = 810;  // 13:30
        private const int AFTERNOON_END = 1050;   // 17:30

        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            if (!int.TryParse(value.ToString(), out int offsetMinutes))
                return false;
            bool isInMorning = offsetMinutes >= MORNING_START && offsetMinutes <= MORNING_END;
            bool isInAfternoon = offsetMinutes >= AFTERNOON_START && offsetMinutes <= AFTERNOON_END;

            return isInMorning || isInAfternoon;
        }
        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be within business hours: 7:30 - 11:30 or 13:30 - 17:30";
        }
        public static bool IsValidBusinessHourSlot(int startOffsetMinutes, int durationMinutes)
        {
            if (startOffsetMinutes < 0 || durationMinutes <= 0)
                return false;

            int endTime = startOffsetMinutes + durationMinutes;

            // Check morning slot: 7:30 - 11:30
            if (startOffsetMinutes >= MORNING_START && endTime <= MORNING_END)
                return true;

            // Check afternoon slot: 13:30 - 17:30
            if (startOffsetMinutes >= AFTERNOON_START && endTime <= AFTERNOON_END)
                return true;

            return false;
        }
    }
}
