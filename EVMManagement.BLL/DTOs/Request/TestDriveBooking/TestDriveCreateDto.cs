using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.TestDriveBooking
{
    
    public class TestDriveCreateDto
    {
        [Required(ErrorMessage = "VehicleTimeslotId is required")]
        public Guid VehicleTimeslotId { get; set; }

        [MaxLength(255, ErrorMessage = "FullName cannot exceed 255 characters")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [MaxLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        [RegularExpression(@"^[0-9+\-\s()]*$", ErrorMessage = "Phone format is invalid")]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        [EmailAddress(ErrorMessage = "Email format is invalid")]
        public string? Email { get; set; }

        public string? Note { get; set; }

       
    }
}
