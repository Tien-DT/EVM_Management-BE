using System;
using System.Collections.Generic;

namespace EVMManagement.BLL.DTOs.Response.VehicleTimeSlot
{
    public class BulkAssignResultDto
    {
        public Guid MasterSlotId { get; set; }
        public DateTime SlotDate { get; set; }
        public int TotalRequested { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<VehicleAssignmentResultDto> Results { get; set; } = new();
    }

    public class VehicleAssignmentResultDto
    {
        public Guid VehicleId { get; set; }
        public bool Success { get; set; }
        public string? Reason { get; set; }
        public Guid? CreatedSlotId { get; set; }
    }
}
