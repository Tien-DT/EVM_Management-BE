using System;
using System.ComponentModel.DataAnnotations;

namespace EVMManagement.BLL.DTOs.Request.Vehicle
{
    public class StockCheckRequestDto
    {
        [Required]
        public Guid VehicleVariantId { get; set; }

        [Required]
        public Guid DealerId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}

