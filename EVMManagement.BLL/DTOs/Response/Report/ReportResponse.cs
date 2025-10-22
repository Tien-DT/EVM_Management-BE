using System;

namespace EVMManagement.BLL.DTOs.Response.Report
{
    public class ReportResponse
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Guid? DealerId { get; set; }
        public string? Type { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? TransportId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
