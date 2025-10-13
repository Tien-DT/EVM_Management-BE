using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EVMManagement.DAL.Data;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Class
{
    public class VehicleModelRepository : GenericRepository<VehicleModel>, IVehicleModelRepository
    {
        public VehicleModelRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<VehicleModel>> GetAllOrderedByCreatedDateDescAsync()
        {
            return await _dbSet.OrderByDescending(m => m.CreatedDate).ToListAsync();
        }

        public async Task<IEnumerable<VehicleModel>> GetByRankingAsync(EVMManagement.DAL.Models.Enums.VehicleModelRanking ranking)
        {
            return await _dbSet.Where(m => m.Ranking == ranking).ToListAsync();
        }


        
    }
}
