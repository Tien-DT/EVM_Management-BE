using System;
using System.ComponentModel.DataAnnotations;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Request.Vehicle
{
    public class VehicleBulkCreateDto
    {
        [Required(ErrorMessage = "VariantId là bắt buộc")]
        public Guid VariantId { get; set; }

        [Required(ErrorMessage = "VIN là bắt buộc")]
        [MaxLength(17, ErrorMessage = "VIN không được vượt quá 17 ký tự")]
        public string Vin { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status là bắt buộc")]
        public VehicleStatus Status { get; set; } = VehicleStatus.IN_STOCK;

        public VehiclePurpose Purpose { get; set; } = VehiclePurpose.FOR_SALE;

        [MaxLength(500, ErrorMessage = "URL hình ảnh không được vượt quá 500 ký tự")]
        public string? ImageUrl { get; set; }
    }
}
