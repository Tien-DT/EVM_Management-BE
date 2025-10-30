using System;

namespace EVMManagement.BLL.DTOs.Request.TransportDetail
{
    public class TransportDetailUpdateDto
    {
        public Guid? TransportId { get; set; }
        public Guid? VehicleId { get; set; }
        public Guid? OrderId { get; set; }
    }
}

