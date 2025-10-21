using Microsoft.EntityFrameworkCore;
using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Repositories.Class
{
    public class TestDriveBookingRepository : GenericRepository<TestDriveBooking>, ITestDriveBookingRepository
    {
        public TestDriveBookingRepository(AppDbContext context) : base(context)
        {
        }

        public IQueryable<TestDriveBooking> GetQueryableWithIncludes()
        {
            return _dbSet
                .Include(x => x.Customer)
                .Include(x => x.VehicleTimeSlot).ThenInclude(vts => vts.Vehicle).ThenInclude(v => v.VehicleVariant).ThenInclude(vv => vv.VehicleModel)
                .Include(x => x.VehicleTimeSlot).ThenInclude(vts => vts.Dealer)
                .Include(x => x.VehicleTimeSlot).ThenInclude(vts => vts.MasterSlot);
        }

        public async Task<TestDriveBooking?> GetByIdWithIncludesAsync(Guid id)
        {
            return await GetQueryableWithIncludes().FirstOrDefaultAsync(x => x.Id == id);
        }

        public IQueryable<TestDriveBooking> GetQueryableWithFilter(Guid? dealerId, Guid? customerId, TestDriveBookingStatus? status)
        {
            var query = GetQueryableWithIncludes();

            if (dealerId.HasValue)
            {
                query = query.Where(x => x.VehicleTimeSlot != null && x.VehicleTimeSlot.DealerId == dealerId.Value);
            }

            if (customerId.HasValue)
            {
                query = query.Where(x => x.CustomerId == customerId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            return query;
        }
    }
}
