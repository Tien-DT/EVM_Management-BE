using System;
using System.Collections.Generic;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.DTOs.Response.Warehouse
{
    public class DealerDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    public class VehicleModelDto
    {
        public string Name { get; init; } = string.Empty;
        public VehicleModelRanking Ranking { get; init; }
        public string? ImageUrl { get; init; }
    }

    public class VehicleVariantDto
    {
        public string? Color { get; init; }
        public VehicleModelDto? VehicleModel { get; init; }
        public string? ImageUrl { get; init; }
    }

    public class VehicleDto
    {
        public Guid Id { get; init; }
        public Guid VariantId { get; init; }
        public string Vin { get; init; } = string.Empty;
        public VehicleStatus Status { get; init; }
        public VehicleVariantDto? Variant { get; init; }
        public string? ImageUrl { get; init; }
    }

    public class WarehouseResponseDto
    {
        public Guid Id { get; init; }
        public Guid? DealerId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Address { get; init; }
        public int? Capacity { get; init; }
        public WarehouseType Type { get; init; }
        public DealerDto? Dealer { get; init; }
        public List<VehicleDto>? Vehicles { get; init; }
        public DateTime CreatedDate { get; init; }
        public DateTime? ModifiedDate { get; init; }
        public DateTime? DeletedDate { get; init; }
        public bool IsDeleted { get; init; }
    }
}