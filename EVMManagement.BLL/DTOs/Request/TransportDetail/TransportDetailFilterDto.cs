using System;

namespace EVMManagement.BLL.DTOs.Request.TransportDetail
{
    public class TransportDetailFilterDto
    {
        public Guid? TransportId { get; set; }
        public Guid? VehicleId { get; set; }
        public Guid? OrderId { get; set; }
        public bool? IsDeleted { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

