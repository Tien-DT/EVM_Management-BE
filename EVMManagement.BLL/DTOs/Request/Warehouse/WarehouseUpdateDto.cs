using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.Warehouse
{
    public class WarehouseUpdateDto
    {
        public Guid? DealerId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Address { get; set; }

        public int? Capacity { get; set; }

        public WarehouseType Type { get; set; }
    }
}
