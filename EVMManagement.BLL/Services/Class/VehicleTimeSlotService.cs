using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.VehicleTimeSlot;
using EVMManagement.BLL.DTOs.Response.VehicleTimeSlot;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;

namespace EVMManagement.BLL.Services.Class
{
    public class VehicleTimeSlotService : IVehicleTimeSlotService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VehicleTimeSlotService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<VehicleTimeSlotResponseDto> CreateVehicleTimeSlotAsync(VehicleTimeSlotCreateDto dto)
        {
            var vehicleTimeSlot = new VehicleTimeSlot
            {
                VehicleId = dto.VehicleId,
                DealerId = dto.DealerId,
                MasterSlotId = dto.MasterSlotId,
                SlotDate = dto.SlotDate,
                Status = dto.Status
            };

            await _unitOfWork.VehicleTimeSlots.AddAsync(vehicleTimeSlot);
            await _unitOfWork.SaveChangesAsync();

            return new VehicleTimeSlotResponseDto
            {
                Id = vehicleTimeSlot.Id,
                VehicleId = vehicleTimeSlot.VehicleId,
                DealerId = vehicleTimeSlot.DealerId,
                MasterSlotId = vehicleTimeSlot.MasterSlotId,
                SlotDate = vehicleTimeSlot.SlotDate,
                Status = vehicleTimeSlot.Status,
                CreatedDate = vehicleTimeSlot.CreatedDate,
                ModifiedDate = vehicleTimeSlot.ModifiedDate,
                IsDeleted = vehicleTimeSlot.IsDeleted
            };
        }
    }
}

