using System;

namespace EVMManagement.BLL.DTOs.Request.HandoverRecord
{
    public class HandoverRecordCreateDto
    {
        public Guid OrderId { get; set; }
        public Guid VehicleId { get; set; }
        public Guid? TransportDetailId { get; set; }
        public string? Notes { get; set; }
        public DateTime? HandoverDate { get; set; }
    }
}
