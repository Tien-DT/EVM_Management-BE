using System;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.TestDriveBooking;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.TestDriveBooking;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using EVMManagement.DAL.UnitOfWork;

namespace EVMManagement.BLL.Services.Class
{
    public class TestDriveBookingService : ITestDriveBookingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TestDriveBookingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TestDriveBookingResponseDto> CreateAsync(TestDriveBookingCreateDto dto)
        {
            var entity = new TestDriveBooking
            {
                VehicleTimeslotId = dto.VehicleTimeslotId,
                CustomerId = dto.CustomerId,
                DealerStaffId = dto.DealerStaffId,
                Status = dto.Status,
                CheckinAt = dto.CheckinAt,
                CheckoutAt = dto.CheckoutAt,
                Note = dto.Note
            };

            await _unitOfWork.TestDriveBookings.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(entity.Id) ?? throw new Exception("Failed to create TestDriveBooking");
        }

        public async Task<PagedResult<TestDriveBookingResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.TestDriveBookings.GetQueryableWithIncludes();
            var total = await _unitOfWork.TestDriveBookings.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(MapToDto).ToList();

            return PagedResult<TestDriveBookingResponseDto>.Create(items, total, pageNumber, pageSize);
        }

       

        public async Task<TestDriveBookingResponseDto?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.TestDriveBookings.GetByIdWithIncludesAsync(id);
            if (entity == null) return null;
            return MapToDto(entity);
        }

        private TestDriveBookingResponseDto MapToDto(TestDriveBooking entity)
        {
            var dto = new TestDriveBookingResponseDto
            {
                Id = entity.Id,
                VehicleTimeslotId = entity.VehicleTimeslotId,
                CustomerId = entity.CustomerId,
                DealerStaffId = entity.DealerStaffId,
                Status = entity.Status,
                CheckinAt = entity.CheckinAt,
                CheckoutAt = entity.CheckoutAt,
                Note = entity.Note,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.ModifiedDate,
                IsDeleted = entity.IsDeleted
            };

            if (entity.Customer != null)
            {
                dto.Customer = new EVMManagement.BLL.DTOs.Response.Customer.CustomerResponse
                {
                    Id = entity.Customer.Id,
                    FullName = entity.Customer.FullName,
                    Phone = entity.Customer.Phone,
                    Email = entity.Customer.Email,
                    CreatedDate = entity.Customer.CreatedDate,
                    ModifiedDate = entity.Customer.ModifiedDate,
                    IsDeleted = entity.Customer.IsDeleted
                };
            }

            if (entity.VehicleTimeSlot != null)
            {
                dto.VehicleTimeSlot = new EVMManagement.BLL.DTOs.Response.VehicleTimeSlot.VehicleTimeSlotResponseDto
                {
                    Id = entity.VehicleTimeSlot.Id,
                    VehicleId = entity.VehicleTimeSlot.VehicleId,
                    DealerId = entity.VehicleTimeSlot.DealerId,
                    MasterSlotId = entity.VehicleTimeSlot.MasterSlotId,
                    SlotDate = entity.VehicleTimeSlot.SlotDate,
                    Status = entity.VehicleTimeSlot.Status,
                    CreatedDate = entity.VehicleTimeSlot.CreatedDate,
                    ModifiedDate = entity.VehicleTimeSlot.ModifiedDate,
                    IsDeleted = entity.VehicleTimeSlot.IsDeleted,
                    Vehicle = entity.VehicleTimeSlot.Vehicle == null ? null : new EVMManagement.BLL.DTOs.Response.Vehicle.VehicleResponseDto
                    {
                        Id = entity.VehicleTimeSlot.Vehicle.Id,
                        VariantId = entity.VehicleTimeSlot.Vehicle.VariantId,
                        WarehouseId = entity.VehicleTimeSlot.Vehicle.WarehouseId,
                        Vin = entity.VehicleTimeSlot.Vehicle.Vin,
                        Status = entity.VehicleTimeSlot.Vehicle.Status,
                        Purpose = entity.VehicleTimeSlot.Vehicle.Purpose,
                        CreatedDate = entity.VehicleTimeSlot.Vehicle.CreatedDate,
                        ModifiedDate = entity.VehicleTimeSlot.Vehicle.ModifiedDate,
                        DeletedDate = entity.VehicleTimeSlot.Vehicle.DeletedDate,
                        IsDeleted = entity.VehicleTimeSlot.Vehicle.IsDeleted
                    },
                    Dealer = entity.VehicleTimeSlot.Dealer == null ? null : new EVMManagement.BLL.DTOs.Response.Dealer.DealerResponseDto
                    {
                        Id = entity.VehicleTimeSlot.Dealer.Id,
                        Name = entity.VehicleTimeSlot.Dealer.Name,
                        Address = entity.VehicleTimeSlot.Dealer.Address,
                        Phone = entity.VehicleTimeSlot.Dealer.Phone,
                        Email = entity.VehicleTimeSlot.Dealer.Email,
                        EstablishedAt = entity.VehicleTimeSlot.Dealer.EstablishedAt,
                        IsActive = entity.VehicleTimeSlot.Dealer.IsActive,
                        CreatedDate = entity.VehicleTimeSlot.Dealer.CreatedDate,
                        ModifiedDate = entity.VehicleTimeSlot.Dealer.ModifiedDate,
                        IsDeleted = entity.VehicleTimeSlot.Dealer.IsDeleted
                    },
                    MasterSlot = entity.VehicleTimeSlot.MasterSlot == null ? null : new EVMManagement.BLL.DTOs.Response.MasterTimeSlot.MasterTimeSlotResponseDto
                    {
                        Id = entity.VehicleTimeSlot.MasterSlot.Id,
                        Code = entity.VehicleTimeSlot.MasterSlot.Code,
                        StartOffsetMinutes = entity.VehicleTimeSlot.MasterSlot.StartOffsetMinutes,
                        DurationMinutes = entity.VehicleTimeSlot.MasterSlot.DurationMinutes,
                        IsActive = entity.VehicleTimeSlot.MasterSlot.IsActive
                    }
                };
            }

            return dto;
        }

       

        public async Task<TestDriveBookingResponseDto?> UpdateAsync(Guid id, TestDriveBookingUpdateDto dto)
        {
            var entity = await _unitOfWork.TestDriveBookings.GetByIdAsync(id);
            if (entity == null) return null;

            if (dto.VehicleTimeslotId.HasValue) entity.VehicleTimeslotId = dto.VehicleTimeslotId.Value;
            if (dto.CustomerId.HasValue) entity.CustomerId = dto.CustomerId.Value;
            if (dto.DealerStaffId.HasValue) entity.DealerStaffId = dto.DealerStaffId.Value;
            if (dto.Status.HasValue) entity.Status = dto.Status.Value;
            if (dto.CheckinAt.HasValue) entity.CheckinAt = dto.CheckinAt;
            if (dto.CheckoutAt.HasValue) entity.CheckoutAt = dto.CheckoutAt;
            if (dto.Note != null) entity.Note = dto.Note;

            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.TestDriveBookings.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<TestDriveBookingResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var entity = await _unitOfWork.TestDriveBookings.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsDeleted = isDeleted;
            entity.ModifiedDate = DateTime.UtcNow;
            entity.DeletedDate = isDeleted ? DateTime.UtcNow : (DateTime?)null;

            _unitOfWork.TestDriveBookings.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.TestDriveBookings.GetByIdAsync(id);
            if (entity == null) return false;

            _unitOfWork.TestDriveBookings.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
