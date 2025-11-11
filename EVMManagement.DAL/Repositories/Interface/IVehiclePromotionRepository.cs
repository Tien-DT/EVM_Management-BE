using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IVehiclePromotionRepository : IGenericRepository<VehiclePromotion>
    {
        IQueryable<VehiclePromotion> GetWithRelations();
    }
}

