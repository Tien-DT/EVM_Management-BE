using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.AvailableSlot;
using EVMManagement.BLL.DTOs.Response.AvailableSlot;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;

namespace EVMManagement.BLL.Services.Class
{
    public class AvailableSlotService : IAvailableSlotService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AvailableSlotService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AvailableSlotResponseDto> CreateAvailableSlotAsync(AvailableSlotCreateDto dto)
        {
            var availableSlot = new AvailableSlot
            {
                VehicleId = dto.VehicleId,
                DealerId = dto.DealerId,
                MasterSlotId = dto.MasterSlotId,
                SlotDate = dto.SlotDate,
                IsAvailable = dto.IsAvailable
            };

            await _unitOfWork.AvailableSlots.AddAsync(availableSlot);
            await _unitOfWork.SaveChangesAsync();

            return new AvailableSlotResponseDto
            {
                Id = availableSlot.Id,
                VehicleId = availableSlot.VehicleId,
                DealerId = availableSlot.DealerId,
                MasterSlotId = availableSlot.MasterSlotId,
                SlotDate = availableSlot.SlotDate,
                IsAvailable = availableSlot.IsAvailable,
                CreatedDate = availableSlot.CreatedDate,
                ModifiedDate = availableSlot.ModifiedDate,
                IsDeleted = availableSlot.IsDeleted
            };
        }
    }
}

