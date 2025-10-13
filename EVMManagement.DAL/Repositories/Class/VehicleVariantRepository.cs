using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Class
{
    public class VehicleVariantRepository : GenericRepository<VehicleVariant>, IVehicleVariantRepository
    {
        public VehicleVariantRepository(AppDbContext context) : base(context)
        {
        }
    }
}
