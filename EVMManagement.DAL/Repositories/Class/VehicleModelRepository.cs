using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<VehicleModel?> UpdateRankingAsync(Guid id, EVMManagement.DAL.Models.Enums.VehicleModelRanking ranking)
        {
            var model = await _dbSet.FindAsync(id);
            if (model == null) return null;
            model.Ranking = ranking;
            model.ModifiedDate = DateTime.UtcNow;
            _dbSet.Update(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<VehicleModel?> UpdateAsync(VehicleModel entity)
        {
            var model = await _dbSet.FindAsync(entity.Id);
            if (model == null) return null;

            // update only mutable fields (exclude Ranking)
            model.Code = entity.Code;
            model.Name = entity.Name;
            model.LaunchDate = entity.LaunchDate;
            model.Description = entity.Description;
            model.Status = entity.Status;

            model.ModifiedDate = DateTime.UtcNow;

            _dbSet.Update(model);
            await _context.SaveChangesAsync();
            return model;
        }

            public async Task<VehicleModel?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
            {
                var model = await _dbSet.FindAsync(id);
                if (model == null) return null;
                model.IsDeleted = isDeleted;
                model.DeletedDate = isDeleted ? DateTime.UtcNow : null;
                model.ModifiedDate = DateTime.UtcNow;
                _dbSet.Update(model);
                await _context.SaveChangesAsync();
                return model;
            }

 
        public async Task<IEnumerable<VehicleModel>> SearchByQueryAsync(string q)
        {
            var qn = q.Trim().ToLowerInvariant();
            var query = _dbSet.AsQueryable();
            query = query.Where(m => (m.Code != null && m.Code.ToLower().Contains(qn))
                                  || (m.Name != null && m.Name.ToLower().Contains(qn)));
            return await query.OrderByDescending(m => m.CreatedDate).ToListAsync();
        }


        
    }
}
