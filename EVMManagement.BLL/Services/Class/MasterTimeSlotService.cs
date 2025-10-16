using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.MasterTimeSlot;
using EVMManagement.BLL.DTOs.Response.MasterTimeSlot;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;

namespace EVMManagement.BLL.Services.Class
{
    public class MasterTimeSlotService : IMasterTimeSlotService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MasterTimeSlotService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<MasterTimeSlotResponseDto> CreateMasterTimeSlotAsync(MasterTimeSlotCreateDto dto)
        {
            var masterTimeSlot = new MasterTimeSlot
            {
                Code = dto.Code,
                StartOffsetMinutes = dto.StartOffsetMinutes,
                DurationMinutes = dto.DurationMinutes,
                IsActive = dto.IsActive
            };

            await _unitOfWork.MasterTimeSlots.AddAsync(masterTimeSlot);
            await _unitOfWork.SaveChangesAsync();

            return new MasterTimeSlotResponseDto
            {
                Id = masterTimeSlot.Id,
                Code = masterTimeSlot.Code,
                StartOffsetMinutes = masterTimeSlot.StartOffsetMinutes,
                DurationMinutes = masterTimeSlot.DurationMinutes,
                IsActive = masterTimeSlot.IsActive
            };
        }
    }
}

