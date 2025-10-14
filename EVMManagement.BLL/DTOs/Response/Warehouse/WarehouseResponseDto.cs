using System;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Response.Warehouse
{
    public class WarehouseResponseDto
    {
        public Guid Id { get; set; }
        public Guid? DealerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public int? Capacity { get; set; }
        public WarehouseType Type { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
