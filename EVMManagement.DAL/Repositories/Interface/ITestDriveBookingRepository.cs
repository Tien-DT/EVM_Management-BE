using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface ITestDriveBookingRepository : IGenericRepository<TestDriveBooking>
    {
        IQueryable<TestDriveBooking> GetQueryableWithIncludes();
        Task<TestDriveBooking?> GetByIdWithIncludesAsync(Guid id);
        IQueryable<TestDriveBooking> GetQueryableWithFilter(Guid? vehicleTimeSlotId, Guid? customerId, Guid? dealerStaffId, TestDriveBookingStatus? status, Guid? dealerId);
    }
}
