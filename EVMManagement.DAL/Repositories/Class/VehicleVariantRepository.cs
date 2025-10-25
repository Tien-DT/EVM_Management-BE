using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Repositories.Class
{
    public class VehicleVariantRepository : GenericRepository<VehicleVariant>, IVehicleVariantRepository
    {
        public VehicleVariantRepository(AppDbContext context) : base(context)
        {
        }

        public IQueryable<VehicleVariant> GetByDealerAndModelAsync(Guid dealerId, Guid modelId)
        {
            return _dbSet
                .Where(v => v.ModelId == modelId && 
                            v.Vehicles.Any(vh => 
                                vh.Warehouse.DealerId == dealerId && 
                                vh.Status == VehicleStatus.IN_STOCK))
                .Distinct();
        }
    }
}
