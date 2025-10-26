using EVMManagement.DAL.Models.Enums;
using System;

namespace EVMManagement.BLL.DTOs.Response.Vehicle
{
    public class VehicleModelWithStockResponseDto
    {
        public Guid Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public DateTime? LaunchDate { get; init; }
        public string? Description { get; init; }
        public string? ImageUrl { get; init; }
        public bool Status { get; init; }
        public VehicleModelRanking? Ranking { get; init; }
        public int AvailableStock { get; init; }
        public DateTime CreatedDate { get; init; }
        public DateTime? ModifiedDate { get; init; }
    }
}
