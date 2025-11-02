using System;

namespace EVMManagement.BLL.DTOs.Response.TestDriveBooking
{
    public class BulkReminderResultDto
    {
        public int TotalRequested { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<ReminderResultDto> Results { get; set; } = new List<ReminderResultDto>();
    }

    public class ReminderResultDto
    {
        public Guid BookingId { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
