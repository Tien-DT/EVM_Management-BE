using System;

namespace EVMManagement.BLL.DTOs.Response.MasterTimeSlot
{
    public class MasterTimeSlotResponseDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int? StartOffsetMinutes { get; set; }
        public int? DurationMinutes { get; set; }
        public bool IsActive { get; set; }
        public Guid? DealerId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}

