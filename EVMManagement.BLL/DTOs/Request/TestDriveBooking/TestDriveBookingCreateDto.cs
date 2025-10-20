using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.TestDriveBooking
{
    public class TestDriveBookingCreateDto
    {
        [Required]
        public Guid VehicleTimeslotId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        public Guid? DealerStaffId { get; set; }

        [Required]
        public TestDriveBookingStatus Status { get; set; }

        public string? Note { get; set; }
    }
}
