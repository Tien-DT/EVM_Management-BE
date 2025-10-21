using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Warehouse;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Warehouse;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IWarehouseService
    {
        Task<WarehouseResponseDto?> GetWarehouseByIdAsync(Guid id);
        Task<PagedResult<WarehouseResponseDto>> GetAllWarehousesAsync(int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<WarehouseResponseDto>> GetWarehousesByDealerIdAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<WarehouseResponseDto>> CreateWarehouseAsync(WarehouseCreateDto dto, AccountRole currentUserRole, Guid? currentUserDealerId = null);
        Task<WarehouseResponseDto?> UpdateWarehouseAsync(Guid id, WarehouseUpdateDto dto);
        Task<WarehouseResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
    }
}
