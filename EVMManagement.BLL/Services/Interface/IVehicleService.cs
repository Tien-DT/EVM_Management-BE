using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Vehicle;
using EVMManagement.DAL.Models.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IVehicleService
    {
        Task<VehicleResponseDto> CreateVehicleAsync(VehicleCreateDto dto);
        Task<PagedResult<VehicleResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<VehicleDetailResponseDto>> GetAllWithDetailsAsync(int pageNumber = 1, int pageSize = 10);
        Task<VehicleResponseDto?> GetByIdAsync(Guid id);
        Task<VehicleResponseDto?> UpdateAsync(Guid id, VehicleUpdateDto dto);
        Task<VehicleResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<PagedResult<VehicleResponseDto>> SearchByQueryAsync(string? q, int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<VehicleResponseDto>> GetByFilterAsync(VehicleFilterDto filter);
        Task<VehicleResponseDto?> UpdateStatusAsync(Guid id, VehicleStatus status);
        Task<StockCheckResponseDto> CheckStockAvailabilityAsync(Guid variantId, Guid dealerId, int quantity);
        Task<PagedResult<VehicleResponseDto>> GetVehiclesByDealerAndVariantAsync(Guid dealerId, Guid variantId, int pageNumber = 1, int pageSize = 10);
    }
}
