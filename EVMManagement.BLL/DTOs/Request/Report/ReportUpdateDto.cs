using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.Report
{
    public class ReportUpdateDto
    {
        public Guid? AccountId { get; set; }

        public Guid? DealerId { get; set; }

        [MaxLength(100)]
        public string? Type { get; set; }

        [MaxLength(255)]
        public string? Title { get; set; }

        public string? Content { get; set; }

        public Guid? OrderId { get; set; }

        public Guid? TransportId { get; set; }
    }
}
