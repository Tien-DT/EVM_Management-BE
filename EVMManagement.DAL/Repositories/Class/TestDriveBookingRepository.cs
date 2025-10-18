using Microsoft.EntityFrameworkCore;
using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.DAL.Repositories.Class
{
    public class TestDriveBookingRepository : GenericRepository<TestDriveBooking>, Repositories.Interface.ITestDriveBookingRepository
    {
        public TestDriveBookingRepository(AppDbContext context) : base(context)
        {
        }

        public IQueryable<TestDriveBooking> GetQueryableWithIncludes()
        {
            return _dbSet
                .Include(x => x.Customer)
                .Include(x => x.VehicleTimeSlot).ThenInclude(vts => vts.Vehicle)
                .Include(x => x.VehicleTimeSlot).ThenInclude(vts => vts.Dealer)
                .Include(x => x.VehicleTimeSlot).ThenInclude(vts => vts.MasterSlot);
        }

        public async Task<TestDriveBooking?> GetByIdWithIncludesAsync(Guid id)
        {
            return await GetQueryableWithIncludes().FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
