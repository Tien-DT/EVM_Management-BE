using System;
using EVMManagement.BLL.DTOs.Response.Customer;
using EVMManagement.BLL.DTOs.Response.VehicleTimeSlot;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Response.TestDriveBooking
{
    public class TestDriveBookingResponseDto
    {
        public Guid Id { get; set; }
        public Guid VehicleTimeslotId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? DealerStaffId { get; set; }
        public TestDriveBookingStatus Status { get; set; }
        public DateTime? CheckinAt { get; set; }
        public DateTime? CheckoutAt { get; set; }
        public string? Note { get; set; }
        public CustomerResponse? Customer { get; set; }
        public VehicleTimeSlotResponseDto? VehicleTimeSlot { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
