using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.TestDriveBooking;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.TestDriveBooking;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.BLL.Services.Interface
{
    public interface ITestDriveBookingService
    {
        Task<TestDriveBookingResponseDto> CreateAsync(TestDriveBookingCreateDto dto);
        Task<PagedResult<TestDriveBookingResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<TestDriveBookingResponseDto?> GetByIdAsync(Guid id);
        Task<PagedResult<TestDriveBookingResponseDto>> GetByFilterAsync(TestDriveBookingFilterDto filterDto);
        Task<TestDriveBookingResponseDto?> UpdateAsync(Guid id, TestDriveBookingUpdateDto dto);
        Task<TestDriveBookingResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<bool> DeleteAsync(Guid id);
        Task SendReminderEmailAsync(Guid bookingId);

        /// <summary>
        /// Tạo lịch hẹn với thông tin khách hàng - tạo khách hàng mới nếu không tìm thấy, sau đó tạo test drive booking
        /// </summary>
        /// <param name="dto">DTO chứa thông tin VehicleTimeSlot, khách hàng, và ghi chú</param>
        /// <returns>TestDriveBookingResponseDto của lịch hẹn vừa được tạo</returns>
        Task<TestDriveBookingResponseDto> CreateWithCustomerInfoAsync(TestDriveCreateDto dto);
    }
}
