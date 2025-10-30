using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.TransportDetail
{
    public class TransportDetailCreateDto
    {
        [Required]
        public Guid TransportId { get; set; }

        [Required]
        public Guid VehicleId { get; set; }

        public Guid? OrderId { get; set; }
    }
}

