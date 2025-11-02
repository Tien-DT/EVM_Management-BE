using System;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.TestDriveBooking;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.TestDriveBooking;
using EVMManagement.BLL.Helpers;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using EVMManagement.DAL.UnitOfWork;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.BLL.Services.Templates;
using EVMManagement.BLL.DTOs.Request.Customer;

namespace EVMManagement.BLL.Services.Class
{
    public class TestDriveBookingService : ITestDriveBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ICustomerService _customerService;

        public TestDriveBookingService(IUnitOfWork unitOfWork, IEmailService emailService, ICustomerService customerService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _customerService = customerService;
        }

        public async Task<TestDriveBookingResponseDto> CreateAsync(TestDriveBookingCreateDto dto)
        {
            // Create the booking entity
            var entity = new TestDriveBooking
            {
                VehicleTimeslotId = dto.VehicleTimeslotId,
                CustomerId = dto.CustomerId,
                DealerStaffId = dto.DealerStaffId,
                Status = dto.Status,
                Note = dto.Note
            };

            await _unitOfWork.TestDriveBookings.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var result = await GetByIdAsync(entity.Id) ?? throw new Exception("Failed to create TestDriveBooking");

            // Update VehicleTimeSlot status to BOOKED if booking status is BOOKED
            if (dto.Status == TestDriveBookingStatus.BOOKED)
            {
                var vehicleTimeSlot = await _unitOfWork.VehicleTimeSlots.GetByIdAsync(dto.VehicleTimeslotId);
                if (vehicleTimeSlot != null)
                {
                    vehicleTimeSlot.Status = TimeSlotStatus.BOOKED;
                    vehicleTimeSlot.ModifiedDate = DateTime.UtcNow;
                    _unitOfWork.VehicleTimeSlots.Update(vehicleTimeSlot);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            // Try to send confirmation email (don't fail the whole operation if email fails)
            try
            {
                await SendTestDriveConfirmationEmailAsync(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to send test drive booking confirmation email: {ex.Message}");
            }

            return result;
        }


        private async Task SendTestDriveConfirmationEmailAsync(TestDriveBookingResponseDto booking)
        {
            if (booking?.Customer == null || booking?.VehicleTimeSlot?.Dealer == null)
            {
                return;
            }

            var customerEmail = booking.Customer.Email;
            if (string.IsNullOrEmpty(customerEmail))
            {
                return;
            }

            var vehicleInfo = BuildVehicleInfo(booking);

            var dealerName = booking.VehicleTimeSlot?.Dealer?.Name ?? "Dealer";
            var dealerPhone = booking.VehicleTimeSlot?.Dealer?.Phone ?? "N/A";
           
            var slotDate = (booking.VehicleTimeSlot?.SlotDate ?? DateTime.UtcNow).Date;
            var startOffsetMinutes = booking.VehicleTimeSlot?.MasterSlot?.StartOffsetMinutes ?? 0;
            var actualAppointmentTime = slotDate.AddMinutes(startOffsetMinutes);

            var subject = "Xác Nhận Lịch Lái Thử - EVM Management";
            var body = EmailTemplates.TestDriveBookingConfirmationEmail(
                booking.Customer.FullName,
                actualAppointmentTime,
                vehicleInfo,
                dealerName,
                dealerPhone
            );

            await _emailService.SendEmailAsync(customerEmail, subject, body, true);
        }

        public async Task<PagedResult<TestDriveBookingResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.TestDriveBookings.GetQueryableWithIncludes();
            var total = await _unitOfWork.TestDriveBookings.CountAsync(x => !x.IsDeleted);

            var entities = await query
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(MapToDto).ToList();

            return PagedResult<TestDriveBookingResponseDto>.Create(items, total, pageNumber, pageSize);
        }


        public async Task<PagedResult<TestDriveBookingResponseDto>> GetByFilterAsync(TestDriveBookingFilterDto filterDto)
        {
            var pageNumber = filterDto?.PageNumber ?? 1;
            var pageSize = filterDto?.PageSize ?? 10;

            var query = _unitOfWork.TestDriveBookings.GetQueryableWithFilter(
                filterDto?.VehicleTimeSlotId, 
                filterDto?.CustomerId, 
                filterDto?.DealerStaffId, 
                filterDto?.Status, 
                filterDto?.DealerId
            );

            var total = await query.CountAsync();

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
                IsDeleted = entity.IsDeleted,
                VehicleModelName = entity.VehicleTimeSlot?.Vehicle?.VehicleVariant?.VehicleModel?.Name,
                VehicleColor = entity.VehicleTimeSlot?.Vehicle?.VehicleVariant?.Color,
                VehicleEngine = entity.VehicleTimeSlot?.Vehicle?.VehicleVariant?.Engine
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

            if (entity.DealerStaff != null)
            {
                dto.DealerStaff = new EVMManagement.BLL.DTOs.Response.User.UserProfileResponse
                {
                    Id = entity.DealerStaff.Id,
                    AccountId = entity.DealerStaff.AccountId,
                    DealerId = entity.DealerStaff.DealerId,
                    FullName = entity.DealerStaff.FullName,
                    Phone = entity.DealerStaff.Phone,
                    CardId = entity.DealerStaff.CardId,
                    CreatedDate = entity.DealerStaff.CreatedDate,
                    ModifiedDate = entity.DealerStaff.ModifiedDate,
                    DeletedDate = entity.DealerStaff.DeletedDate,
                    IsDeleted = entity.DealerStaff.IsDeleted
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
            if (dto.CheckinAt.HasValue)
            {
                entity.CheckinAt = DateTimeHelper.ToUtc(dto.CheckinAt);
                entity.Status = TestDriveBookingStatus.CHECKED_IN;
            }

            if (dto.CheckoutAt.HasValue)
            {
                entity.CheckoutAt = DateTimeHelper.ToUtc(dto.CheckoutAt);
                entity.Status = TestDriveBookingStatus.COMPLETED;
            }

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


        public async Task SendReminderEmailAsync(Guid bookingId)
        {
            try
            {
                var booking = await GetByIdAsync(bookingId);
                if (booking == null)
                {
                    throw new Exception($"Booking {bookingId} not found");
                }
                if (booking.Status != TestDriveBookingStatus.BOOKED)
                {
                    throw new Exception($"Booking {bookingId} has status {booking.Status}. Only BOOKED bookings can receive reminder emails.");
                }

                if (booking?.Customer == null || booking?.VehicleTimeSlot?.Dealer == null)
                {
                    throw new Exception("Invalid booking data: Customer or Dealer information missing");
                }

                var customerEmail = booking.Customer.Email;
                if (string.IsNullOrEmpty(customerEmail))
                {
                    throw new Exception("Customer email is missing");
                }

                var vehicleInfo = BuildVehicleInfo(booking);

                var dealerName = booking.VehicleTimeSlot?.Dealer?.Name ?? "Dealer";
                var dealerPhone = booking.VehicleTimeSlot?.Dealer?.Phone ?? "N/A";
                var dealerAddress = booking.VehicleTimeSlot?.Dealer?.Address ?? "N/A";

                var actualAppointmentTime = GetActualAppointmentTime(booking);

                var subject = "📍 Nhắc Nhở: Lịch Lái Thử Sắp Diễn Ra - EVM Management";
                var body = EVMManagement.BLL.Services.Templates.EmailTemplates.TestDriveReminderEmail(
                    booking.Customer.FullName,
                    actualAppointmentTime,
                    vehicleInfo,
                    dealerName,
                    dealerPhone,
                    dealerAddress
                );

                await _emailService.SendEmailAsync(customerEmail, subject, body, true);

                System.Diagnostics.Debug.WriteLine($"Reminder email sent for booking {bookingId} at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending reminder email: {ex.Message}");
                throw;
            }
        }

       
        public async Task<BulkReminderResultDto> BulkSendReminderEmailAsync(List<Guid> bookingIds)
        {
            var result = new BulkReminderResultDto
            {
                TotalRequested = bookingIds.Count,
                Results = new List<ReminderResultDto>()
            };

            var tasks = bookingIds.Select(async bookingId =>
            {
                var reminderResult = new ReminderResultDto
                {
                    BookingId = bookingId,
                    Success = false
                };

                try
                {
                    await SendReminderEmailAsync(bookingId);
                    reminderResult.Success = true;
                }
                catch (Exception ex)
                {
                    reminderResult.Success = false;
                    reminderResult.ErrorMessage = ex.Message;
                }

                return reminderResult;
            });

            var results = await Task.WhenAll(tasks);
            result.Results = results.ToList();
            result.SuccessCount = results.Count(r => r.Success);
            result.FailureCount = results.Count(r => !r.Success);

            return result;
        }

        private string BuildVehicleInfo(TestDriveBookingResponseDto booking)
        {
            if (booking == null)
            {
                return "Thông Tin Xe";
            }

            var parts = new System.Collections.Generic.List<string>();
            
            if (!string.IsNullOrEmpty(booking.VehicleModelName))
                parts.Add(booking.VehicleModelName);
                
            if (!string.IsNullOrEmpty(booking.VehicleColor))
                parts.Add(booking.VehicleColor);
                
            if (!string.IsNullOrEmpty(booking.VehicleEngine))
                parts.Add(booking.VehicleEngine);

            if (parts.Count > 0)
            {
                return string.Join(" - ", parts);
            }

            return booking.VehicleTimeSlot?.Vehicle != null
                ? $"VIN: {booking.VehicleTimeSlot.Vehicle.Vin}"
                : "Thông Tin Xe";
        }

        private DateTime GetActualAppointmentTime(TestDriveBookingResponseDto booking)
        {
            var slotDate = (booking.VehicleTimeSlot?.SlotDate ?? DateTime.UtcNow).Date;
            var startOffsetMinutes = booking.VehicleTimeSlot?.MasterSlot?.StartOffsetMinutes ?? 0;

            var actualAppointmentTime = slotDate.AddMinutes(startOffsetMinutes);

            System.Diagnostics.Debug.WriteLine(
                $"Booking {booking.Id}: SlotDate={slotDate:yyyy-MM-dd}, " +
                $"StartOffsetMinutes={startOffsetMinutes}min, " +
                $"ActualAppointmentTime={actualAppointmentTime:yyyy-MM-dd HH:mm:ss}"
            );

            return actualAppointmentTime;
        }
    }
}
