using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.AvailableSlot;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.AvailableSlot;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IAvailableSlotService
    {
        Task<AvailableSlotResponseDto> CreateAvailableSlotAsync(AvailableSlotCreateDto dto);
        Task<PagedResult<AvailableSlotResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<AvailableSlotResponseDto?> GetByIdAsync(Guid id);
        Task<PagedResult<AvailableSlotResponseDto>> GetByVehicleIdAsync(Guid vehicleId, int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<AvailableSlotResponseDto>> GetByDealerIdAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<AvailableSlotResponseDto>> GetAvailableAsync(int pageNumber = 1, int pageSize = 10);
        Task<AvailableSlotResponseDto?> UpdateAsync(Guid id, AvailableSlotUpdateDto dto);
        Task<AvailableSlotResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<bool> DeleteAsync(Guid id);
    }
}

