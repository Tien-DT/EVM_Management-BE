using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface ITestDriveBookingRepository : IGenericRepository<TestDriveBooking>
    {
        IQueryable<TestDriveBooking> GetQueryableWithIncludes();
        Task<TestDriveBooking?> GetByIdWithIncludesAsync(Guid id);
    }
}
