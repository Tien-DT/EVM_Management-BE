using System;

namespace EVMManagement.BLL.DTOs.Response.TransportDetail
{
    public class TransportDetailResponseDto
    {
        public Guid Id { get; set; }
        public Guid TransportId { get; set; }
        public Guid VehicleId { get; set; }
        public string? VehicleVin { get; set; }
        public string? VehicleVariantName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}

