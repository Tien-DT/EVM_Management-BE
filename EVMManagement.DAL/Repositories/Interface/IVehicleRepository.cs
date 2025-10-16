using EVMManagement.DAL.Models.Entities;
using System.Linq;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        IQueryable<Vehicle> SearchByQueryAsync(string q);
    }
}
