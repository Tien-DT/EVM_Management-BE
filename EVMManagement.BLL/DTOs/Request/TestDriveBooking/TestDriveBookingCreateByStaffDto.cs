using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.TestDriveBooking
{
    public class TestDriveBookingCreateByStaffDto
    {
        public DateTime BookingDate { get; set; }

        public Guid MasterSlotId { get; set; }

        public Guid VehicleId { get; set; }

        public Guid DealerId { get; set; }

        [Required(ErrorMessage = "Số điện thoại khách hàng là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [MaxLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên khách hàng là bắt buộc")]
        [MaxLength(255)]
        public string CustomerFullName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [MaxLength(255)]
        public string? CustomerEmail { get; set; }

        [MaxLength(20)]
        public string? CustomerGender { get; set; }

        [MaxLength(255)]
        public string? CustomerAddress { get; set; }

        public DateTime? CustomerDob { get; set; }

        [MaxLength(50)]
        public string? CustomerCardId { get; set; }

        public string? Note { get; set; }

        public Guid? DealerStaffId { get; set; }
    }
}
