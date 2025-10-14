using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Vehicle;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IVehicleModelService
    {
        Task<VehicleModelResponseDto> CreateVehicleModelAsync(VehicleModelCreateDto dto);
        Task<PagedResult<VehicleModelResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<VehicleModelResponseDto>> GetByRankingAsync(VehicleModelRanking ranking, int pageNumber = 1, int pageSize = 10);
        Task<VehicleModelResponseDto?> UpdateVehicleModelAsync(Guid id, VehicleModelUpdateDto dto);
        Task<VehicleModelResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<IEnumerable<VehicleModelResponseDto>> SearchByQueryAsync(string? q);
    }
}
