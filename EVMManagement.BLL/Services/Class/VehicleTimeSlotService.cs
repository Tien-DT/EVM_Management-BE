using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVMManagement.BLL.DTOs.Request.VehicleTimeSlot;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.VehicleTimeSlot;
using EVMManagement.BLL.Helpers;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.BLL.Services.Class
{
    public class VehicleTimeSlotService : IVehicleTimeSlotService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VehicleTimeSlotService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<VehicleTimeSlotResponseDto> CreateVehicleTimeSlotAsync(VehicleTimeSlotCreateDto dto)
        {
            if (dto.Status == TimeSlotStatus.BOOKED)
            {
                var availableSlot = _unitOfWork.VehicleTimeSlots.GetQueryable()
                    .FirstOrDefault(x => x.VehicleId == dto.VehicleId
                                      && x.DealerId == dto.DealerId
                                      && x.MasterSlotId == dto.MasterSlotId
                                      && x.SlotDate.Date == dto.SlotDate.Date
                                      && x.Status == TimeSlotStatus.AVAILABLE);

                if (availableSlot != null)
                {
                    availableSlot.Status = TimeSlotStatus.BOOKED;
                    availableSlot.ModifiedDate = DateTime.UtcNow;
                    _unitOfWork.VehicleTimeSlots.Update(availableSlot);
                }
            }

            var vehicleTimeSlot = new VehicleTimeSlot
            {
                VehicleId = dto.VehicleId,
                DealerId = dto.DealerId,
                MasterSlotId = dto.MasterSlotId,
                SlotDate = DateTimeHelper.ToUtc(dto.SlotDate),
                Status = dto.Status
            };

            await _unitOfWork.VehicleTimeSlots.AddAsync(vehicleTimeSlot);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<VehicleTimeSlotResponseDto>(vehicleTimeSlot);
        }

        public async Task<PagedResult<VehicleTimeSlotResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.VehicleTimeSlots.GetQueryable();
            var totalCount = await _unitOfWork.VehicleTimeSlots.CountAsync();

            var items = await query
                .OrderByDescending(x => x.SlotDate)
                .ThenByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<VehicleTimeSlotResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<VehicleTimeSlotResponseDto>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<VehicleTimeSlotResponseDto?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.VehicleTimeSlots.GetByIdAsync(id);
            if (entity == null) return null;

            return _mapper.Map<VehicleTimeSlotResponseDto>(entity);
        }

        public async Task<PagedResult<VehicleTimeSlotResponseDto>> GetByVehicleIdAsync(Guid vehicleId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.VehicleTimeSlots.GetQueryable()
                .Where(x => x.VehicleId == vehicleId);

            var totalCount = query.Count();

            var items = await query
                .OrderByDescending(x => x.SlotDate)
                .ThenByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<VehicleTimeSlotResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<VehicleTimeSlotResponseDto>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<VehicleTimeSlotResponseDto>> GetByDealerIdAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.VehicleTimeSlots.GetQueryable()
                .Where(x => x.DealerId == dealerId);

            var totalCount = query.Count();

            var items = await query
                .OrderByDescending(x => x.SlotDate)
                .ThenByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<VehicleTimeSlotResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<VehicleTimeSlotResponseDto>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<VehicleTimeSlotResponseDto>> GetByStatusAsync(TimeSlotStatus status, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.VehicleTimeSlots.GetQueryable()
                .Where(x => x.Status == status);

            var totalCount = query.Count();

            var items = await query
                .OrderByDescending(x => x.SlotDate)
                .ThenByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<VehicleTimeSlotResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return PagedResult<VehicleTimeSlotResponseDto>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<VehicleTimeSlotResponseDto?> UpdateAsync(Guid id, VehicleTimeSlotUpdateDto dto)
        {
            var entity = await _unitOfWork.VehicleTimeSlots.GetByIdAsync(id);
            if (entity == null) return null;

            if (dto.VehicleId.HasValue) entity.VehicleId = dto.VehicleId.Value;
            if (dto.DealerId.HasValue) entity.DealerId = dto.DealerId.Value;
            if (dto.MasterSlotId.HasValue) entity.MasterSlotId = dto.MasterSlotId.Value;
            if (dto.SlotDate.HasValue) entity.SlotDate = DateTimeHelper.ToUtc(dto.SlotDate.Value);
            if (dto.Status.HasValue) entity.Status = dto.Status.Value;

            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.VehicleTimeSlots.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<VehicleTimeSlotResponseDto?> UpdateStatusAsync(Guid id, TimeSlotStatus status)
        {
            var entity = await _unitOfWork.VehicleTimeSlots.GetByIdAsync(id);
            if (entity == null) return null;

            entity.Status = status;
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.VehicleTimeSlots.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<VehicleTimeSlotResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var entity = await _unitOfWork.VehicleTimeSlots.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsDeleted = isDeleted;
            if (isDeleted)
            {
                entity.DeletedDate = DateTime.UtcNow;
            }
            else
            {
                entity.DeletedDate = null;
            }
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.VehicleTimeSlots.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.VehicleTimeSlots.GetByIdAsync(id);
            if (entity == null) return false;

            _unitOfWork.VehicleTimeSlots.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<List<DaySlotsummaryDto>> GetAvailableSlotByModelAsync(
            Guid modelId, Guid dealerId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            DateTime? queryFromDate = fromDate;
            DateTime? queryToDate = toDate;
            
            if (fromDate.HasValue && !toDate.HasValue)
            {
                queryToDate = fromDate.Value.Date;
            }
            else if (!fromDate.HasValue && toDate.HasValue)
            {
                queryFromDate = toDate.Value.Date;
            }

            var slotSummariesByDate = await _unitOfWork.VehicleTimeSlots.GetSlotSummariesByModelAsync(
                modelId, dealerId, 
                queryFromDate?.Date, 
                queryToDate?.Date);

            if (slotSummariesByDate.Count == 0) return new List<DaySlotsummaryDto>();

            var results = slotSummariesByDate
                .Select(dateGroup => new DaySlotsummaryDto
                {
                    SlotDate = dateGroup.Key,
                    Slots = dateGroup.Value.Select(slot => new SlotSummaryDto
                    {
                        MasterSlotId = slot.MasterSlotId,
                        AvailableCount = slot.AvailableCount,
                        StartOffsetMinutes = slot.StartOffsetMinutes,
                        DurationMinutes = slot.DurationMinutes
                    }).ToList()
                })
                .OrderBy(d => d.SlotDate)
                .ToList();

            return results;
        }

        public async Task<SlotVehiclesDto?> GetAvailableVehiclesBySlotAsync(
            Guid modelId, Guid dealerId, DateTime slotDate, Guid masterSlotId)
        {
            var date = slotDate.Date;

            var availableVehicles = await _unitOfWork.VehicleTimeSlots.GetAvailableVehiclesBySlotAsync(
                modelId, dealerId, date, masterSlotId);

            if (availableVehicles.Count == 0)
            {
                return null;
            }

            var masterSlot = await _unitOfWork.MasterTimeSlots.GetByIdAsync(masterSlotId);
            if (masterSlot == null) return null;

            var vehicleDtos = availableVehicles
                .Select(v => new VehicleDetailDto { VehicleId = v.VehicleId, Vin = v.Vin })
                .ToList();

            return new SlotVehiclesDto
            {
                SlotDate = date,
                MasterSlotId = masterSlotId,
                AvailableCount = vehicleDtos.Count,
                Vehicles = vehicleDtos,
                StartOffsetMinutes = masterSlot.StartOffsetMinutes ?? 0,
                DurationMinutes = masterSlot.DurationMinutes ?? 0
            };
        }

       
    }
}

