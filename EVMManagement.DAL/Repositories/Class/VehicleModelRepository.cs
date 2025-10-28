using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Repositories.Class
{
    public class VehicleModelRepository : GenericRepository<VehicleModel>, IVehicleModelRepository
    {
        public VehicleModelRepository(AppDbContext context) : base(context)
        {
        }


        public IQueryable<VehicleModel> GetByRankingAsync(VehicleModelRanking ranking)
        {
            return _dbSet.Where(m => m.Ranking == ranking);
        }

        public IQueryable<VehicleModel> SearchByQueryAsync(string q)
        {
            var qn = q.Trim().ToLowerInvariant();
            var query = _dbSet.AsQueryable();
            query = query.Where(m => (m.Code != null && m.Code.ToLower().Contains(qn))
                                  || (m.Name != null && m.Name.ToLower().Contains(qn)));
            return query;
        }

        public IQueryable<VehicleModel> GetByDealerAsync(Guid dealerId)
        {
            return _dbSet
                .Where(m => m.VehicleVariants.Any(v => v.Vehicles.Any(vh => 
                    vh.Warehouse.DealerId == dealerId && 
                    vh.Status == VehicleStatus.IN_STOCK)))
                .Distinct();
        }

        
    }
}
