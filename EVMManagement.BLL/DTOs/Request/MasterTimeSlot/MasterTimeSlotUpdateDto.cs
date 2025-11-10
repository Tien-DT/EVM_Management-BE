using System.ComponentModel.DataAnnotations;
using EVMManagement.BLL.Helpers;

namespace EVMManagement.BLL.DTOs.Request.MasterTimeSlot
{
    public class MasterTimeSlotUpdateDto : IValidatableObject
    {
        [MaxLength(50)]
        public string? Code { get; set; }

        [Range(0, 1439, ErrorMessage = "StartOffsetMinutes must be between 0 and 1439 (24 hours)")]
        public int? StartOffsetMinutes { get; set; }

        [Range(1, 1440, ErrorMessage = "DurationMinutes must be between 1 and 1440 (24 hours)")]
        public int? DurationMinutes { get; set; }

        public bool? IsActive { get; set; }
      
        public Guid? DealerId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (StartOffsetMinutes.HasValue && DurationMinutes.HasValue)
            {
                if (!BusinessHoursValidation.IsValidBusinessHourSlot(StartOffsetMinutes.Value, DurationMinutes.Value))
                {
                    yield return new ValidationResult(
                        "Time slot must fall within business hours: 7:30 - 11:30 or 13:30 - 17:30",
                        new[] { nameof(StartOffsetMinutes) }
                    );
                }
            }
        }
    }
}

