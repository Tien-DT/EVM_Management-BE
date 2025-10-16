using System;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.MasterTimeSlot;
using EVMManagement.BLL.DTOs.Response;
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

        public Task<PagedResult<MasterTimeSlotResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.MasterTimeSlots.GetQueryable();
            var totalCount = query.Count();

            var items = query
                .OrderBy(x => x.StartOffsetMinutes)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new MasterTimeSlotResponseDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    StartOffsetMinutes = x.StartOffsetMinutes,
                    DurationMinutes = x.DurationMinutes,
                    IsActive = x.IsActive
                })
                .ToList();

            return Task.FromResult(PagedResult<MasterTimeSlotResponseDto>.Create(items, totalCount, pageNumber, pageSize));
        }

        public async Task<MasterTimeSlotResponseDto?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.MasterTimeSlots.GetByIdAsync(id);
            if (entity == null) return null;

            return new MasterTimeSlotResponseDto
            {
                Id = entity.Id,
                Code = entity.Code,
                StartOffsetMinutes = entity.StartOffsetMinutes,
                DurationMinutes = entity.DurationMinutes,
                IsActive = entity.IsActive
            };
        }

        public Task<PagedResult<MasterTimeSlotResponseDto>> GetActiveAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.MasterTimeSlots.GetQueryable()
                .Where(x => x.IsActive);

            var totalCount = query.Count();

            var items = query
                .OrderBy(x => x.StartOffsetMinutes)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new MasterTimeSlotResponseDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    StartOffsetMinutes = x.StartOffsetMinutes,
                    DurationMinutes = x.DurationMinutes,
                    IsActive = x.IsActive
                })
                .ToList();

            return Task.FromResult(PagedResult<MasterTimeSlotResponseDto>.Create(items, totalCount, pageNumber, pageSize));
        }

        public async Task<MasterTimeSlotResponseDto?> UpdateAsync(Guid id, MasterTimeSlotUpdateDto dto)
        {
            var entity = await _unitOfWork.MasterTimeSlots.GetByIdAsync(id);
            if (entity == null) return null;

            if (dto.Code != null) entity.Code = dto.Code;
            if (dto.StartOffsetMinutes.HasValue) entity.StartOffsetMinutes = dto.StartOffsetMinutes;
            if (dto.DurationMinutes.HasValue) entity.DurationMinutes = dto.DurationMinutes;
            if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;

            _unitOfWork.MasterTimeSlots.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<MasterTimeSlotResponseDto?> UpdateIsActiveAsync(Guid id, bool isActive)
        {
            var entity = await _unitOfWork.MasterTimeSlots.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsActive = isActive;

            _unitOfWork.MasterTimeSlots.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.MasterTimeSlots.GetByIdAsync(id);
            if (entity == null) return false;

            _unitOfWork.MasterTimeSlots.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}

