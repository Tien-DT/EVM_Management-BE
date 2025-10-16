using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.MasterTimeSlot;
using EVMManagement.BLL.DTOs.Response.MasterTimeSlot;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IMasterTimeSlotService
    {
        Task<MasterTimeSlotResponseDto> CreateMasterTimeSlotAsync(MasterTimeSlotCreateDto dto);
    }
}

