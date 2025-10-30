using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Warehouse;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Vehicle;
using EVMManagement.BLL.DTOs.Response.Warehouse;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IWarehouseService
    {
        Task<WarehouseResponseDto?> GetWarehouseByIdAsync(Guid id);
        Task<PagedResult<WarehouseResponseDto>> GetAllWarehousesAsync(int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<WarehouseResponseDto>> GetWarehousesByDealerIdAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<WarehouseResponseDto>> GetWarehousesByTypeAsync(WarehouseType type, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<WarehouseResponseDto>> CreateWarehouseAsync(WarehouseCreateDto dto, AccountRole currentUserRole, Guid? currentUserDealerId = null);
        Task<ApiResponse<WarehouseResponseDto>> UpdateWarehouseAsync(Guid id, WarehouseUpdateDto dto, AccountRole currentUserRole, Guid? currentUserDealerId = null);
        Task<ApiResponse<WarehouseResponseDto>> UpdateIsDeletedAsync(Guid id, bool isDeleted, AccountRole currentUserRole, Guid? currentUserDealerId = null);
        Task<ApiResponse<List<VehicleResponseDto>>> AddVehiclesToWarehouseAsync(AddVehiclesToWarehouseRequestDto dto, Guid addedByUserId);
        Task<ApiResponse<List<VehicleResponseDto>>> AddVehiclesToEvmWarehouseAsync(AddVehiclesToEvmWarehouseDto dto, Guid addedByUserId);
        Task<ApiResponse<List<VehicleResponseDto>>> AddVehiclesToDealerWarehouseAsync(AddVehiclesToDealerWarehouseDto dto, Guid addedByUserId);
        IQueryable<Warehouse> GetQueryableForOData();
    }
}
