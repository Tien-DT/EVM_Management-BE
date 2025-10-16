using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.VehicleTimeSlot;
using EVMManagement.BLL.DTOs.Response.VehicleTimeSlot;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IVehicleTimeSlotService
    {
        Task<VehicleTimeSlotResponseDto> CreateVehicleTimeSlotAsync(VehicleTimeSlotCreateDto dto);
    }
}

