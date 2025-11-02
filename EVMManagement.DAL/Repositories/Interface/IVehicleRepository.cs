using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        IQueryable<Vehicle> SearchByQueryAsync(string q);
        IQueryable<Vehicle> GetVehiclesByDealerAndVariantAsync(Guid dealerId, Guid variantId);
        Task<List<Vehicle>> GetTestDriveVehiclesByDealerAsync(Guid dealerId);
    }
}
