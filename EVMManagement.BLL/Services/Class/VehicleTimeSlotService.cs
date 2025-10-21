using System;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.VehicleTimeSlot;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.VehicleTimeSlot;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
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

        public async Task<PagedResult<VehicleTimeSlotResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.VehicleTimeSlots.GetQueryable();
            var totalCount = await _unitOfWork.VehicleTimeSlots.CountAsync();

            var items = query
                .OrderByDescending(x => x.SlotDate)
                .ThenByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new VehicleTimeSlotResponseDto
                {
                    Id = x.Id,
                    VehicleId = x.VehicleId,
                    DealerId = x.DealerId,
                    MasterSlotId = x.MasterSlotId,
                    SlotDate = x.SlotDate,
                    Status = x.Status,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    IsDeleted = x.IsDeleted
                })
                .ToList();

            return PagedResult<VehicleTimeSlotResponseDto>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<VehicleTimeSlotResponseDto?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.VehicleTimeSlots.GetByIdAsync(id);
            if (entity == null) return null;

            return new VehicleTimeSlotResponseDto
            {
                Id = entity.Id,
                VehicleId = entity.VehicleId,
                DealerId = entity.DealerId,
                MasterSlotId = entity.MasterSlotId,
                SlotDate = entity.SlotDate,
                Status = entity.Status,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.ModifiedDate,
                IsDeleted = entity.IsDeleted
            };
        }

        public Task<PagedResult<VehicleTimeSlotResponseDto>> GetByVehicleIdAsync(Guid vehicleId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.VehicleTimeSlots.GetQueryable()
                .Where(x => x.VehicleId == vehicleId);

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(x => x.SlotDate)
                .ThenByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new VehicleTimeSlotResponseDto
                {
                    Id = x.Id,
                    VehicleId = x.VehicleId,
                    DealerId = x.DealerId,
                    MasterSlotId = x.MasterSlotId,
                    SlotDate = x.SlotDate,
                    Status = x.Status,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    IsDeleted = x.IsDeleted
                })
                .ToList();

            return Task.FromResult(PagedResult<VehicleTimeSlotResponseDto>.Create(items, totalCount, pageNumber, pageSize));
        }

        public Task<PagedResult<VehicleTimeSlotResponseDto>> GetByDealerIdAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.VehicleTimeSlots.GetQueryable()
                .Where(x => x.DealerId == dealerId);

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(x => x.SlotDate)
                .ThenByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new VehicleTimeSlotResponseDto
                {
                    Id = x.Id,
                    VehicleId = x.VehicleId,
                    DealerId = x.DealerId,
                    MasterSlotId = x.MasterSlotId,
                    SlotDate = x.SlotDate,
                    Status = x.Status,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    IsDeleted = x.IsDeleted
                })
                .ToList();

            return Task.FromResult(PagedResult<VehicleTimeSlotResponseDto>.Create(items, totalCount, pageNumber, pageSize));
        }

        public Task<PagedResult<VehicleTimeSlotResponseDto>> GetByStatusAsync(TimeSlotStatus status, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.VehicleTimeSlots.GetQueryable()
                .Where(x => x.Status == status);

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(x => x.SlotDate)
                .ThenByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new VehicleTimeSlotResponseDto
                {
                    Id = x.Id,
                    VehicleId = x.VehicleId,
                    DealerId = x.DealerId,
                    MasterSlotId = x.MasterSlotId,
                    SlotDate = x.SlotDate,
                    Status = x.Status,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    IsDeleted = x.IsDeleted
                })
                .ToList();

            return Task.FromResult(PagedResult<VehicleTimeSlotResponseDto>.Create(items, totalCount, pageNumber, pageSize));
        }

        public async Task<VehicleTimeSlotResponseDto?> UpdateAsync(Guid id, VehicleTimeSlotUpdateDto dto)
        {
            var entity = await _unitOfWork.VehicleTimeSlots.GetByIdAsync(id);
            if (entity == null) return null;

            if (dto.VehicleId.HasValue) entity.VehicleId = dto.VehicleId.Value;
            if (dto.DealerId.HasValue) entity.DealerId = dto.DealerId.Value;
            if (dto.MasterSlotId.HasValue) entity.MasterSlotId = dto.MasterSlotId.Value;
            if (dto.SlotDate.HasValue) entity.SlotDate = dto.SlotDate.Value;
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
    }
}

