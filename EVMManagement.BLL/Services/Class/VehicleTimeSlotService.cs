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

       
        
       
       
        

       

        /// <summary>
        /// Lấy danh sách ngày và các slot có sẵn trong mỗi ngày cho một variant
        /// Trả về: Ngày → Danh sách slot (slot ID, số xe trống, thời gian)
        /// </summary>
        public async Task<List<DaySlotsummaryDto>> GetAvailableSlotByVariantAsync(
            Guid modelId, Guid variantId, Guid dealerId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            // Normalize dates to date-only, use today as default
            var from = (fromDate ?? DateTime.UtcNow).Date;
            var to = (toDate ?? DateTime.UtcNow).Date;

            // Get all vehicles of this variant at the dealer
            var allVehicles = await _unitOfWork.VehicleTimeSlots.GetAvailableVehiclesByVariantAndDealerAsync(variantId, dealerId);

            // Get booked/available VehicleTimeSlots for this variant in date range
            var bookedBySlot = await _unitOfWork.VehicleTimeSlots.GetSlotsByVariantInDateRangeAsync(modelId, variantId, dealerId, from, to);

            // Group by slot
            var bookedGroupedBySlot = bookedBySlot
                .GroupBy(s => new { s.SlotDate.Date, s.MasterSlotId, s.MasterSlot.StartOffsetMinutes, s.MasterSlot.DurationMinutes })
                .Select(g => new
                {
                    SlotDate = g.Key.Date,
                    MasterSlotId = g.Key.MasterSlotId,
                    StartOffsetMinutes = g.Key.StartOffsetMinutes ?? 0,
                    DurationMinutes = g.Key.DurationMinutes ?? 0,
                    BookedVehicleIds = g.Where(x => x.Status == TimeSlotStatus.BOOKED).Select(x => x.VehicleId).ToHashSet()
                })
                .ToList();

            var results = new List<DaySlotsummaryDto>();

            // Group by date
            var groupedByDate = bookedGroupedBySlot
                .GroupBy(b => b.SlotDate)
                .OrderBy(g => g.Key);

            foreach (var dateGroup in groupedByDate)
            {
                var slots = new List<SlotSummaryDto>();

                foreach (var slot in dateGroup)
                {
                    // Count available vehicles for this slot (not booked in this slot)
                    var availableCount = allVehicles.Count(v => !slot.BookedVehicleIds.Contains(v.Id));

                    slots.Add(new SlotSummaryDto
                    {
                        MasterSlotId = slot.MasterSlotId,
                        AvailableCount = availableCount,
                        StartOffsetMinutes = slot.StartOffsetMinutes,
                        DurationMinutes = slot.DurationMinutes
                    });
                }

                results.Add(new DaySlotsummaryDto
                {
                    SlotDate = dateGroup.Key,
                    Slots = slots.OrderBy(s => s.StartOffsetMinutes).ToList()
                });
            }

            return results;
        }

        /// <summary>
        /// Lấy danh sách xe trống của một time slot cụ thể trong một ngày
        /// Trả về: Ngày + Slot + Danh sách xe trống lịch trong slot đó
        /// </summary>
        public async Task<SlotVehiclesDto?> GetAvailableVehiclesBySlotAsync(
            Guid modelId, Guid variantId, Guid dealerId, DateTime slotDate, Guid masterSlotId)
        {
            // Normalize date to date-only
            var date = slotDate.Date;

            // Get all vehicles of this variant at the dealer
            var allVehicles = await _unitOfWork.VehicleTimeSlots.GetAvailableVehiclesByVariantAndDealerAsync(variantId, dealerId);
            
            if (allVehicles.Count == 0) return null;

            // Get VehicleTimeSlots for this specific slot from repository
            var slotsData = await _unitOfWork.VehicleTimeSlots.GetSlotsByDateAndMasterSlotAsync(
                modelId, variantId, dealerId, date, masterSlotId);

            if (slotsData.Count == 0)
            {
                // Slot has no bookings yet, all vehicles are available
                // Get MasterSlot info
                var masterSlot = await _unitOfWork.MasterTimeSlots.GetQueryable()
                    .FirstOrDefaultAsync(m => m.Id == masterSlotId);
                
                if (masterSlot == null) return null;

                return new SlotVehiclesDto
                {
                    SlotDate = date,
                    MasterSlotId = masterSlotId,
                    AvailableCount = allVehicles.Count,
                    Vehicles = allVehicles.Select(v => new VehicleDetailDto { VehicleId = v.Id, Vin = v.Vin }).ToList(),
                    StartOffsetMinutes = masterSlot.StartOffsetMinutes ?? 0,
                    DurationMinutes = masterSlot.DurationMinutes ?? 0
                };
            }

            // Get booked vehicle IDs in this slot
            var bookedVehicleIds = slotsData
                .Where(x => x.Status == TimeSlotStatus.BOOKED)
                .Select(x => x.VehicleId)
                .ToHashSet();

            // Get available vehicles (not booked in this slot)
            var availableVehicles = allVehicles
                .Where(v => !bookedVehicleIds.Contains(v.Id))
                .Select(v => new VehicleDetailDto { VehicleId = v.Id, Vin = v.Vin })
                .ToList();

            var masterSlotInfo = slotsData.First().MasterSlot;

            return new SlotVehiclesDto
            {
                SlotDate = date,
                MasterSlotId = masterSlotId,
                AvailableCount = availableVehicles.Count,
                Vehicles = availableVehicles,
                StartOffsetMinutes = masterSlotInfo.StartOffsetMinutes ?? 0,
                DurationMinutes = masterSlotInfo.DurationMinutes ?? 0
            };
        }

       
    }
}

