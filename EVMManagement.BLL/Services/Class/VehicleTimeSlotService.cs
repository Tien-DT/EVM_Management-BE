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

        public async Task<BulkAssignResultDto> BulkAssignVehiclesToSlotAsync(BulkAssignVehiclesDto dto, Guid dealerId)
        {
            var result = new BulkAssignResultDto
            {
                MasterSlotId = dto.MasterSlotId,
                SlotDate = dto.SlotDate.Date,
                TotalRequested = dto.VehicleIds.Count,
                Results = new List<VehicleAssignmentResultDto>()
            };

            var masterSlot = await _unitOfWork.MasterTimeSlots.GetByIdAsync(dto.MasterSlotId);
            if (masterSlot == null)
            {
                foreach (var vehicleId in dto.VehicleIds)
                {
                    result.Results.Add(new VehicleAssignmentResultDto
                    {
                        VehicleId = vehicleId,
                        Success = false,
                        Reason = "MasterSlot not found"
                    });
                }
                result.FailureCount = result.TotalRequested;
                return result;
            }

            var existingVehicleIds = await _unitOfWork.VehicleTimeSlots
                .GetExistingVehicleIdsForSlotDateAsync(dto.VehicleIds, dto.SlotDate.Date);

            var vehicles = await _unitOfWork.VehicleTimeSlots
                .GetVehiclesByIdsAsync(dto.VehicleIds);
            var vehicleDict = vehicles.ToDictionary(v => v.Id);

            var slotsToCreate = new List<VehicleTimeSlot>();

            foreach (var vehicleId in dto.VehicleIds)
            {
                if (existingVehicleIds.Contains(vehicleId))
                {
                    result.Results.Add(new VehicleAssignmentResultDto
                    {
                        VehicleId = vehicleId,
                        Success = false,
                        Reason = "Vehicle already has a slot on this date"
                    });
                    continue;
                }

                if (!vehicleDict.TryGetValue(vehicleId, out var vehicle))
                {
                    result.Results.Add(new VehicleAssignmentResultDto
                    {
                        VehicleId = vehicleId,
                        Success = false,
                        Reason = "Vehicle not found"
                    });
                    continue;
                }

                if (vehicle.Status != VehicleStatus.IN_STOCK)
                {
                    result.Results.Add(new VehicleAssignmentResultDto
                    {
                        VehicleId = vehicleId,
                        Success = false,
                        Reason = $"Vehicle status is {vehicle.Status}, must be IN_STOCK"
                    });
                    continue;
                }

                if (vehicle.Purpose != VehiclePurpose.TEST_DRIVE)
                {
                    result.Results.Add(new VehicleAssignmentResultDto
                    {
                        VehicleId = vehicleId,
                        Success = false,
                        Reason = $"Vehicle purpose is {vehicle.Purpose}, must be TEST_DRIVE"
                    });
                    continue;
                }

                var newSlot = new VehicleTimeSlot
                {
                    VehicleId = vehicleId,
                    DealerId = dealerId,
                    MasterSlotId = dto.MasterSlotId,
                    SlotDate = DateTimeHelper.ToUtc(dto.SlotDate.Date),
                    Status = dto.Status,
                    CreatedDate = DateTime.UtcNow
                };

                slotsToCreate.Add(newSlot);

                result.Results.Add(new VehicleAssignmentResultDto
                {
                    VehicleId = vehicleId,
                    Success = true,
                    CreatedSlotId = newSlot.Id
                });
            }

            if (slotsToCreate.Any())
            {
                await _unitOfWork.VehicleTimeSlots.BulkAddAsync(slotsToCreate);
                await _unitOfWork.SaveChangesAsync();
            }

            result.SuccessCount = result.Results.Count(r => r.Success);
            result.FailureCount = result.Results.Count(r => !r.Success);

            return result;
        }

        public async Task<List<DateSlotGroupDto>> GetSlotsByDateAsync(
            Guid dealerId,
            DateTime fromDate,
            DateTime toDate,
            TimeSlotStatus? status = null
            )
        {
            // Use repository method to get slots
            var slots = await _unitOfWork.VehicleTimeSlots.GetSlotsByDateRangeAsync(
                dealerId, fromDate, toDate, status);

            // Group by Date -> MasterSlotId -> Vehicles
            var grouped = slots
                .GroupBy(vts => vts.SlotDate.Date)
                .Select(dateGroup => new DateSlotGroupDto
                {
                    Date = dateGroup.Key.ToString("yyyy-MM-dd"),
                    TotalSlots = dateGroup.Select(vts => vts.MasterSlotId).Distinct().Count(), // Count distinct MasterTimeSlots
                    TotalVehiclesAvailable = dateGroup.Count(s => s.Status == TimeSlotStatus.AVAILABLE),
                    TotalVehiclesBooked = dateGroup.Count(s => s.Status == TimeSlotStatus.BOOKED),
                    
                    // Group by MasterSlotId within each date
                    MasterSlots = dateGroup
                        .GroupBy(vts => vts.MasterSlotId)
                        .Select(masterSlotGroup => 
                        {
                            var firstSlot = masterSlotGroup.First();
                            var startMinutes = firstSlot.MasterSlot.StartOffsetMinutes ?? 0;
                            var duration = firstSlot.MasterSlot.DurationMinutes ?? 60;
                            
                            return new MasterSlotWithVehiclesDto
                            {
                                MasterSlotId = firstSlot.MasterSlotId,
                                MasterSlotCode = firstSlot.MasterSlot.Code,
                                StartOffsetMinutes = startMinutes,
                                DurationMinutes = duration,
                                StartTime = FormatTime(startMinutes),
                                EndTime = FormatTime(startMinutes + duration),
                                IsActive = firstSlot.MasterSlot.IsActive,
                                TotalVehicles = masterSlotGroup.Count(),
                                AvailableVehicles = masterSlotGroup.Count(s => s.Status == TimeSlotStatus.AVAILABLE),
                                BookedVehicles = masterSlotGroup.Count(s => s.Status == TimeSlotStatus.BOOKED),
                                
                                // Map vehicles in this time slot
                                Vehicles = masterSlotGroup
                                    .Where(vts => vts.Vehicle != null)
                                    .Select(vts => new VehicleTimeSlotDetailDto
                                    {
                                        VehicleTimeSlotId = vts.Id,
                                        Status = vts.Status,
                                        Vehicle = new VehicleSummaryDetailDto
                                        {
                                            Id = vts.Vehicle.Id,
                                            Vin = vts.Vehicle.Vin,
                                            ModelName = vts.Vehicle.VehicleVariant?.VehicleModel?.Name ?? "N/A",
                                            
                                            Status = vts.Vehicle.Status,
                                            Purpose = vts.Vehicle.Purpose,
                                            Color = vts.Vehicle.VehicleVariant?.Color
                                           
                                        }
                                    })
                                    .ToList()
                            };
                        })
                        .OrderBy(ms => ms.StartOffsetMinutes)
                        .ToList()
                })
                .OrderBy(d => d.Date)
                .ToList();

            return grouped;
        }

        private string FormatTime(int? minutes)
        {
            if (!minutes.HasValue) return "N/A";
            var hours = minutes.Value / 60;
            var mins = minutes.Value % 60;
            return $"{hours:D2}:{mins:D2}";
        }

        public async Task<AvailableVehiclesForSlotDto> GetAvailableVehiclesForSlotAsync(
            Guid dealerId, 
            DateTime slotDate, 
            Guid masterSlotId)
        {
            var result = new AvailableVehiclesForSlotDto
            {
                DealerId = dealerId,
                SlotDate = slotDate,
                MasterSlotId = masterSlotId
            };

            // Delegate queries to repositories
            var allTestDriveVehicles = await _unitOfWork.Vehicles.GetTestDriveVehiclesByDealerAsync(dealerId);

            var assignedVehicleIds = await _unitOfWork.VehicleTimeSlots.GetAssignedVehicleIdsForSlotAsync(
                dealerId, slotDate.Date, masterSlotId);

            // Map vehicles and mark which ones are already assigned
            result.AvailableVehicles = allTestDriveVehicles.Select(v => new VehicleForAssignmentDto
            {
                Id = v.Id,
                Vin = v.Vin,
                ModelName = v.VehicleVariant?.VehicleModel?.Name ?? "Unknown Model",
                
                Color = v.VehicleVariant?.Color ?? "N/A",
                ImageUrl = v.ImageUrl,
                IsAlreadyAssigned = assignedVehicleIds.Contains(v.Id)
            })
            .OrderBy(v => v.IsAlreadyAssigned)
            .ThenBy(v => v.ModelName)
            .ToList();

            result.TotalAvailable = result.AvailableVehicles.Count(v => !v.IsAlreadyAssigned);
            result.AlreadyAssigned = result.AvailableVehicles.Count(v => v.IsAlreadyAssigned);

            return result;
        }
    }
}

