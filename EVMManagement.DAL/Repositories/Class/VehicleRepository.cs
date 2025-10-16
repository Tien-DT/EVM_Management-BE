using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;
using System.Linq;

namespace EVMManagement.DAL.Repositories.Class
{
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(AppDbContext context) : base(context)
        {
        }

        public IQueryable<Vehicle> SearchByQueryAsync(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return GetQueryable();
            q = q.ToLower();
            return GetQueryable().Where(v => v.Vin.ToLower().Contains(q));
        }
    }
}
