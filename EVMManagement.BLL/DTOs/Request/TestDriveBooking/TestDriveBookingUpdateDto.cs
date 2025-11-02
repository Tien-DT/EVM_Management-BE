using System;

namespace EVMManagement.BLL.DTOs.Request.TestDriveBooking
{
    public class TestDriveBookingUpdateDto
    {
        public DateTime? CheckinAt { get; set; }
        public DateTime? CheckoutAt { get; set; }
    }
}
