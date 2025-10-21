using System;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.TestDriveBooking
{
    public class TestDriveBookingUpdateDto
    {
        public Guid? VehicleTimeslotId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? DealerStaffId { get; set; }
        public TestDriveBookingStatus? Status { get; set; }
        public DateTime? CheckinAt { get; set; }
        public DateTime? CheckoutAt { get; set; }
        public string? Note { get; set; }
    }
}
