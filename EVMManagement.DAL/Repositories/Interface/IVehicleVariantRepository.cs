using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IVehicleVariantRepository : IGenericRepository<VehicleVariant>
    {
        IQueryable<VehicleVariant> GetByDealerAndModelAsync(Guid dealerId, Guid modelId);
    }
}
