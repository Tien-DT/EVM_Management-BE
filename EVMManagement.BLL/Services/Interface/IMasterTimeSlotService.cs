using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.MasterTimeSlot;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.MasterTimeSlot;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IMasterTimeSlotService
    {
        Task<MasterTimeSlotResponseDto> CreateMasterTimeSlotAsync(MasterTimeSlotCreateDto dto);
        Task<PagedResult<MasterTimeSlotResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<MasterTimeSlotResponseDto?> GetByIdAsync(Guid id);
        Task<PagedResult<MasterTimeSlotResponseDto>> GetActiveAsync(int pageNumber = 1, int pageSize = 10);
    }
}

