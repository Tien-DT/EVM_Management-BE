using System;
using EVMManagement.BLL.DTOs.Response.Order;
using EVMManagement.BLL.DTOs.Response.Transport;
using EVMManagement.BLL.DTOs.Response.Vehicle;

namespace EVMManagement.BLL.DTOs.Response.HandoverRecord
{
    public class HandoverRecordResponseDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid VehicleId { get; set; }
        public Guid? TransportId { get; set; }
        public string? Notes { get; set; }
        public string? FileUrl { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime? HandoverDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public OrderResponse? Order { get; set; }
        public VehicleResponseDto? Vehicle { get; set; }
        public TransportResponseDto? Transport { get; set; }
    }
}
