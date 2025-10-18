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
    }
}
