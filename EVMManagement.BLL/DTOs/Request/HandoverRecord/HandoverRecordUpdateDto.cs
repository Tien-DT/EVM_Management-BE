using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.HandoverRecord
{
    public class HandoverRecordUpdateDto
    {
        public Guid? TransportId { get; set; }
        public string? Notes { get; set; }
        [MaxLength(500)]
        public string? FileUrl { get; set; }
        public bool? IsAccepted { get; set; }
        public DateTime? HandoverDate { get; set; }
    }
}
