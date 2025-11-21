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
        private const int MAX_ACTIVE_SLOTS_PER_DEALER = 4;

        public MasterTimeSlotService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<MasterTimeSlotResponseDto> CreateMasterTimeSlotAsync(MasterTimeSlotCreateDto dto)
        {
            var masterTimeSlot = _mapper.Map<MasterTimeSlot>(dto);

            if (masterTimeSlot.IsActive && masterTimeSlot.DealerId.HasValue)
            {
                var activeSlots = await _unitOfWork.MasterTimeSlots.GetActiveByDealerIdAsync(masterTimeSlot.DealerId.Value);
                if (activeSlots.Count() >= MAX_ACTIVE_SLOTS_PER_DEALER)
                {
                    throw new InvalidOperationException(
                        $"Cannot create a new active master time slot. You already have {MAX_ACTIVE_SLOTS_PER_DEALER} active slots. " +
                        $"Please inactivate one of the unused slots before creating a new one.");
                }
            }

            await _unitOfWork.MasterTimeSlots.AddAsync(masterTimeSlot);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<MasterTimeSlotResponseDto>(masterTimeSlot);
        }

        public Task<PagedResult<MasterTimeSlotResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.MasterTimeSlots.GetQueryable()
                .Where(x => !x.IsDeleted);
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
                .Where(x => x.IsActive && !x.IsDeleted);

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

            if (entity.IsActive && entity.DealerId.HasValue && (dto.IsActive == true))
            {
                var activeSlots = await _unitOfWork.MasterTimeSlots.GetActiveByDealerIdAsync(entity.DealerId.Value);
                var activeCount = activeSlots.Count(s => s.Id != id);

                if (activeCount >= MAX_ACTIVE_SLOTS_PER_DEALER)
                {
                    throw new InvalidOperationException(
                        $"Cannot activate this master time slot. You already have {MAX_ACTIVE_SLOTS_PER_DEALER} active slots. " +
                        $"Please inactivate one of the unused slots before activating this one.");
                }
            }

            entity.ModifiedDate = DateTime.UtcNow;
            _unitOfWork.MasterTimeSlots.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<MasterTimeSlotResponseDto>(entity);
        }

        public async Task<MasterTimeSlotResponseDto?> UpdateIsActiveAsync(Guid id, bool isActive)
        {
            var entity = await _unitOfWork.MasterTimeSlots.GetByIdAsync(id);
            if (entity == null) return null;

            if (isActive && entity.DealerId.HasValue)
            {
                var activeSlots = await _unitOfWork.MasterTimeSlots.GetActiveByDealerIdAsync(entity.DealerId.Value);
                var activeCount = activeSlots.Count(s => s.Id != id);

                if (activeCount >= MAX_ACTIVE_SLOTS_PER_DEALER)
                {
                    throw new InvalidOperationException(
                        $"Cannot activate this master time slot. You already have {MAX_ACTIVE_SLOTS_PER_DEALER} active slots. " +
                        $"Please inactivate one of the unused slots before activating this one.");
                }
            }

            entity.IsActive = isActive;
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.MasterTimeSlots.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.MasterTimeSlots.GetByIdAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.DeletedDate = DateTime.UtcNow;
            
            _unitOfWork.MasterTimeSlots.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<PagedResult<MasterTimeSlotResponseDto>> GetByDealerIdAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10)
        {
            var items = await _unitOfWork.MasterTimeSlots.GetByDealerIdAsync(dealerId);
            var totalCount = items.Count();

            var pagedItems = items
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var responses = _mapper.Map<List<MasterTimeSlotResponseDto>>(pagedItems);

            return PagedResult<MasterTimeSlotResponseDto>.Create(responses, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<MasterTimeSlotResponseDto>> GetActiveByDealerIdAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10)
        {
            var items = await _unitOfWork.MasterTimeSlots.GetActiveByDealerIdAsync(dealerId);
            var totalCount = items.Count();

            var pagedItems = items
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var responses = _mapper.Map<List<MasterTimeSlotResponseDto>>(pagedItems);

            return PagedResult<MasterTimeSlotResponseDto>.Create(responses, totalCount, pageNumber, pageSize);
        }
    }
}

