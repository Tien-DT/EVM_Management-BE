using System;

namespace EVMManagement.BLL.DTOs.Response.HandoverRecord
{
    public class HandoverRecordResponseDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid VehicleId { get; set; }
        public Guid? TransportDetailId { get; set; }
        public string? Notes { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime? HandoverDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
