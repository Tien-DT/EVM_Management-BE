using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface ITestDriveBookingRepository : IGenericRepository<TestDriveBooking>
    {
        IQueryable<TestDriveBooking> GetQueryableWithIncludes();
        Task<TestDriveBooking?> GetByIdWithIncludesAsync(Guid id);
        IQueryable<TestDriveBooking> GetQueryableWithFilter(Guid? vehicleTimeSlotId, string? customerPhone, Guid? dealerStaffId, TestDriveBookingStatus? status, Guid? dealerId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<TestDriveBooking?> GetExistingBookingAsync(Guid vehicleTimeSlotId, Guid customerId);
    }
}
