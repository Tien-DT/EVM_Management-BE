using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.AvailableSlot;
using EVMManagement.BLL.DTOs.Response.AvailableSlot;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IAvailableSlotService
    {
        Task<AvailableSlotResponseDto> CreateAvailableSlotAsync(AvailableSlotCreateDto dto);
    }
}

