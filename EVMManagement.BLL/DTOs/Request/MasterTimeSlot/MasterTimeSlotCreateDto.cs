using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.MasterTimeSlot
{
    public class MasterTimeSlotCreateDto
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Range(0, 1439, ErrorMessage = "StartOffsetMinutes must be between 0 and 1439 (24 hours)")]
        public int? StartOffsetMinutes { get; set; }

        [Range(1, 1440, ErrorMessage = "DurationMinutes must be between 1 and 1440 (24 hours)")]
        public int? DurationMinutes { get; set; }

        public bool IsActive { get; set; } = true;
   
        public Guid? DealerId { get; set; }
    }
}

