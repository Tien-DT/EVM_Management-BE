using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.DTOs.Response.Vehicle;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IVehicleModelService
    {
        Task<VehicleModelResponseDto> CreateVehicleModelAsync(VehicleModelCreateDto dto);
        Task<IEnumerable<VehicleModelResponseDto>> GetAllAsync();
        Task<IEnumerable<VehicleModelResponseDto>> GetByRankingAsync(VehicleModelRanking ranking);
        Task<VehicleModelResponseDto?> UpdateVehicleModelAsync(Guid id, VehicleModelUpdateDto dto);
    }
}
