using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Models.Entities
{
    public class TestDriveBooking : BaseEntity
    {
        [Required]
        public Guid VehicleTimeslotId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        public Guid? DealerStaffId { get; set; }

        [Required]
        public TestDriveBookingStatus Status { get; set; }

        public DateTime? CheckinAt { get; set; }

        public DateTime? CheckoutAt { get; set; }

        public string? Note { get; set; }
        public virtual VehicleTimeSlot VehicleTimeSlot { get; set; } = null!;
        public virtual Customer Customer { get; set; } = null!;
        public virtual UserProfile? DealerStaff { get; set; }
    }
}
