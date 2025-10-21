using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public MasterTimeSlotService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<MasterTimeSlotResponseDto> CreateMasterTimeSlotAsync(MasterTimeSlotCreateDto dto)
        {
            var masterTimeSlot = _mapper.Map<MasterTimeSlot>(dto);

            await _unitOfWork.MasterTimeSlots.AddAsync(masterTimeSlot);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<MasterTimeSlotResponseDto>(masterTimeSlot);
        }

        public Task<PagedResult<MasterTimeSlotResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.MasterTimeSlots.GetQueryable();
            var totalCount = query.Count();

            var items = query
                .OrderBy(x => x.StartOffsetMinutes)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var responses = _mapper.Map<List<MasterTimeSlotResponseDto>>(items);

            return Task.FromResult(PagedResult<MasterTimeSlotResponseDto>.Create(responses, totalCount, pageNumber, pageSize));
        }

        public async Task<MasterTimeSlotResponseDto?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.MasterTimeSlots.GetByIdAsync(id);
            if (entity == null) return null;

            return _mapper.Map<MasterTimeSlotResponseDto>(entity);
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
                .ToList();

            var responses = _mapper.Map<List<MasterTimeSlotResponseDto>>(items);

            return Task.FromResult(PagedResult<MasterTimeSlotResponseDto>.Create(responses, totalCount, pageNumber, pageSize));
        }

        public async Task<MasterTimeSlotResponseDto?> UpdateAsync(Guid id, MasterTimeSlotUpdateDto dto)
        {
            var entity = await _unitOfWork.MasterTimeSlots.GetByIdAsync(id);
            if (entity == null) return null;

            _mapper.Map(dto, entity);

            _unitOfWork.MasterTimeSlots.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<MasterTimeSlotResponseDto>(entity);
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

