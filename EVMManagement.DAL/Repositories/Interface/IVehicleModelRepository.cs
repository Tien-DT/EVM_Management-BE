using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IVehicleModelRepository : IGenericRepository<VehicleModel>
    {

        IQueryable<VehicleModel> GetByRankingAsync(VehicleModelRanking ranking);
        IQueryable<VehicleModel> SearchByQueryAsync(string q);
    }
}
