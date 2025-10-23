using EVMManagement.DAL.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        IQueryable<Vehicle> SearchByQueryAsync(string q);
        Task<List<(Guid Id, string Name, int VariantCount)>> GetModelsByDealerAsync(Guid dealerId);
        Task<List<(Guid Id, Guid ModelId, string ModelName,int AvailableCount)>> GetVariantsByDealerAndModelAsync(Guid dealerId, Guid modelId);
    }
}
