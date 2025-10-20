using System;

namespace EVMManagement.BLL.DTOs.Request.HandoverRecord
{
    public class HandoverRecordUpdateDto
    {
        public Guid? TransportDetailId { get; set; }
        public string? Notes { get; set; }
        public bool? IsAccepted { get; set; }
        public DateTime? HandoverDate { get; set; }
    }
}
