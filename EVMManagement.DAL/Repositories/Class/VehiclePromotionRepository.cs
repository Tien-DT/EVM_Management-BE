using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.DAL.Repositories.Class
{
    public class VehiclePromotionRepository : GenericRepository<VehiclePromotion>, IVehiclePromotionRepository
    {
        public VehiclePromotionRepository(AppDbContext context) : base(context)
        {
        }

        public IQueryable<VehiclePromotion> GetWithRelations()
        {
            return _dbSet
                .Include(vp => vp.VehicleVariant)
                .ThenInclude(vv => vv.VehicleModel)
                .Include(vp => vp.Promotion);
        }
    }
}

