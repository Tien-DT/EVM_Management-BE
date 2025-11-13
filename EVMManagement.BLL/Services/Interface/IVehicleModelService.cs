using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Vehicle;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.Models.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IVehicleModelService
    {
        Task<VehicleModelResponseDto> CreateVehicleModelAsync(VehicleModelCreateDto dto);
        Task<PagedResult<VehicleModelResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<VehicleModelResponseDto?> GetByIdAsync(Guid id);
        Task<PagedResult<VehicleModelResponseDto>> GetByRankingAsync(VehicleModelRanking ranking, int pageNumber = 1, int pageSize = 10);
        Task<VehicleModelResponseDto?> UpdateVehicleModelAsync(Guid id, VehicleModelUpdateDto dto);
        Task<VehicleModelResponseDto?> SoftDeleteAsync(Guid id);
        Task<PagedResult<VehicleModelResponseDto>> SearchByQueryAsync(string? q, int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<VehicleModelResponseDto>> GetByDealerAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<VehicleModelWithStockResponseDto>> GetAllWithDealerStockAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10);
        IQueryable<VehicleModel> GetQueryableForOData();
    }
}
