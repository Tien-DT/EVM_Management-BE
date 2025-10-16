using System;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.AvailableSlot;
using EVMManagement.BLL.DTOs.Response;
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
            // Kiểm tra xem đã tồn tại AvailableSlot cho cùng vehicle, dealer, master slot và date chưa
            var existingSlot = _unitOfWork.AvailableSlots.GetQueryable()
                .FirstOrDefault(x => x.VehicleId == dto.VehicleId
                                  && x.DealerId == dto.DealerId
                                  && x.MasterSlotId == dto.MasterSlotId
                                  && x.SlotDate.Date == dto.SlotDate.Date
                                  && !x.IsDeleted);

            if (existingSlot != null)
            {
                throw new InvalidOperationException("AvailableSlot already exists for this vehicle, dealer, master slot and date");
            }

            // Kiểm tra xem có VehicleTimeSlot đang BOOKED hoặc PENDING không
            var bookedSlot = _unitOfWork.VehicleTimeSlots.GetQueryable()
                .FirstOrDefault(x => x.VehicleId == dto.VehicleId
                                  && x.DealerId == dto.DealerId
                                  && x.MasterSlotId == dto.MasterSlotId
                                  && x.SlotDate.Date == dto.SlotDate.Date
                                  && (x.Status == DAL.Models.Enums.TimeSlotStatus.BOOKED 
                                      || x.Status == DAL.Models.Enums.TimeSlotStatus.PENDING)
                                  && !x.IsDeleted);

            // Nếu có slot đang booked, tự động set IsAvailable = false
            var isAvailable = dto.IsAvailable && bookedSlot == null;

            var availableSlot = new AvailableSlot
            {
                VehicleId = dto.VehicleId,
                DealerId = dto.DealerId,
                MasterSlotId = dto.MasterSlotId,
                SlotDate = dto.SlotDate,
                IsAvailable = isAvailable
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

        public async Task<PagedResult<AvailableSlotResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.AvailableSlots.GetQueryable();
            var totalCount = await _unitOfWork.AvailableSlots.CountAsync();

            var items = query
                .OrderByDescending(x => x.SlotDate)
                .ThenByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AvailableSlotResponseDto
                {
                    Id = x.Id,
                    VehicleId = x.VehicleId,
                    DealerId = x.DealerId,
                    MasterSlotId = x.MasterSlotId,
                    SlotDate = x.SlotDate,
                    IsAvailable = x.IsAvailable,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    IsDeleted = x.IsDeleted
                })
                .ToList();

            return PagedResult<AvailableSlotResponseDto>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<AvailableSlotResponseDto?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.AvailableSlots.GetByIdAsync(id);
            if (entity == null) return null;

            return new AvailableSlotResponseDto
            {
                Id = entity.Id,
                VehicleId = entity.VehicleId,
                DealerId = entity.DealerId,
                MasterSlotId = entity.MasterSlotId,
                SlotDate = entity.SlotDate,
                IsAvailable = entity.IsAvailable,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.ModifiedDate,
                IsDeleted = entity.IsDeleted
            };
        }

        public Task<PagedResult<AvailableSlotResponseDto>> GetByVehicleIdAsync(Guid vehicleId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.AvailableSlots.GetQueryable()
                .Where(x => x.VehicleId == vehicleId);

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(x => x.SlotDate)
                .ThenByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AvailableSlotResponseDto
                {
                    Id = x.Id,
                    VehicleId = x.VehicleId,
                    DealerId = x.DealerId,
                    MasterSlotId = x.MasterSlotId,
                    SlotDate = x.SlotDate,
                    IsAvailable = x.IsAvailable,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    IsDeleted = x.IsDeleted
                })
                .ToList();

            return Task.FromResult(PagedResult<AvailableSlotResponseDto>.Create(items, totalCount, pageNumber, pageSize));
        }

        public Task<PagedResult<AvailableSlotResponseDto>> GetByDealerIdAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.AvailableSlots.GetQueryable()
                .Where(x => x.DealerId == dealerId);

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(x => x.SlotDate)
                .ThenByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AvailableSlotResponseDto
                {
                    Id = x.Id,
                    VehicleId = x.VehicleId,
                    DealerId = x.DealerId,
                    MasterSlotId = x.MasterSlotId,
                    SlotDate = x.SlotDate,
                    IsAvailable = x.IsAvailable,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    IsDeleted = x.IsDeleted
                })
                .ToList();

            return Task.FromResult(PagedResult<AvailableSlotResponseDto>.Create(items, totalCount, pageNumber, pageSize));
        }

        public Task<PagedResult<AvailableSlotResponseDto>> GetAvailableAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.AvailableSlots.GetQueryable()
                .Where(x => x.IsAvailable);

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(x => x.SlotDate)
                .ThenByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AvailableSlotResponseDto
                {
                    Id = x.Id,
                    VehicleId = x.VehicleId,
                    DealerId = x.DealerId,
                    MasterSlotId = x.MasterSlotId,
                    SlotDate = x.SlotDate,
                    IsAvailable = x.IsAvailable,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    IsDeleted = x.IsDeleted
                })
                .ToList();

            return Task.FromResult(PagedResult<AvailableSlotResponseDto>.Create(items, totalCount, pageNumber, pageSize));
        }

        public async Task<AvailableSlotResponseDto?> UpdateAsync(Guid id, AvailableSlotUpdateDto dto)
        {
            var entity = await _unitOfWork.AvailableSlots.GetByIdAsync(id);
            if (entity == null) return null;

            entity.VehicleId = dto.VehicleId;
            entity.DealerId = dto.DealerId;
            entity.MasterSlotId = dto.MasterSlotId;
            entity.SlotDate = dto.SlotDate;
            entity.IsAvailable = dto.IsAvailable;
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.AvailableSlots.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return new AvailableSlotResponseDto
            {
                Id = entity.Id,
                VehicleId = entity.VehicleId,
                DealerId = entity.DealerId,
                MasterSlotId = entity.MasterSlotId,
                SlotDate = entity.SlotDate,
                IsAvailable = entity.IsAvailable,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.ModifiedDate,
                IsDeleted = entity.IsDeleted
            };
        }

        public async Task<AvailableSlotResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var entity = await _unitOfWork.AvailableSlots.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsDeleted = isDeleted;
            entity.DeletedDate = isDeleted ? DateTime.UtcNow : null;
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.AvailableSlots.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return new AvailableSlotResponseDto
            {
                Id = entity.Id,
                VehicleId = entity.VehicleId,
                DealerId = entity.DealerId,
                MasterSlotId = entity.MasterSlotId,
                SlotDate = entity.SlotDate,
                IsAvailable = entity.IsAvailable,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.ModifiedDate,
                IsDeleted = entity.IsDeleted
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.AvailableSlots.GetByIdAsync(id);
            if (entity == null) return false;

            _unitOfWork.AvailableSlots.Delete(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}

