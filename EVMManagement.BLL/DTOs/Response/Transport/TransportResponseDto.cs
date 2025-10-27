using System;
using System.Collections.Generic;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Response.Transport
{
    public class TransportResponseDto
    {
        public Guid Id { get; set; }
        public string? ProviderName { get; set; }
        public string? PickupLocation { get; set; }
        public string? DropoffLocation { get; set; }
        public TransportStatus Status { get; set; }
        public DateTime? ScheduledPickupAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        
        public List<TransportDetailDto> TransportDetails { get; set; } = new List<TransportDetailDto>();
    }

    public class TransportDetailDto
    {
        public Guid Id { get; set; }
        public Guid TransportId { get; set; }
        public Guid VehicleId { get; set; }
        public Guid? OrderId { get; set; }
        
        // Vehicle info
        public string? VehicleVin { get; set; }
        public string? VehicleVariantName { get; set; }
        
        // Order info
        public string? OrderCode { get; set; }
    }
}

