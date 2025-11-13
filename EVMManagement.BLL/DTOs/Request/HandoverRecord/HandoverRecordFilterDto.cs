using System;

namespace EVMManagement.BLL.DTOs.Request.HandoverRecord
{
    public class HandoverRecordFilterDto
    {
        public Guid? OrderId { get; set; }
        public Guid? VehicleId { get; set; }
        public Guid? TransportId { get; set; }
        public Guid? DealerId { get; set; }
        public bool? IsAccepted { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
