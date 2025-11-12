using System;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.TestDriveBooking
{
    public class TestDriveBookingFilterDto
    {
        public Guid? VehicleTimeSlotId { get; set; }
        public string? CustomerPhone { get; set; }
        public Guid? DealerStaffId { get; set; }
        public TestDriveBookingStatus? Status { get; set; }
        public Guid? DealerId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
