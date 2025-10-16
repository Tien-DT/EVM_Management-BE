using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.VehicleTimeSlot;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.VehicleTimeSlot;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IVehicleTimeSlotService
    {
        Task<VehicleTimeSlotResponseDto> CreateVehicleTimeSlotAsync(VehicleTimeSlotCreateDto dto);
        Task<PagedResult<VehicleTimeSlotResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<VehicleTimeSlotResponseDto?> GetByIdAsync(Guid id);
        Task<PagedResult<VehicleTimeSlotResponseDto>> GetByVehicleIdAsync(Guid vehicleId, int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<VehicleTimeSlotResponseDto>> GetByDealerIdAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<VehicleTimeSlotResponseDto>> GetByStatusAsync(TimeSlotStatus status, int pageNumber = 1, int pageSize = 10);
        Task<VehicleTimeSlotResponseDto?> UpdateAsync(Guid id, VehicleTimeSlotUpdateDto dto);
        Task<VehicleTimeSlotResponseDto?> UpdateStatusAsync(Guid id, TimeSlotStatus status);
        Task<VehicleTimeSlotResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<bool> DeleteAsync(Guid id);
    }
}

