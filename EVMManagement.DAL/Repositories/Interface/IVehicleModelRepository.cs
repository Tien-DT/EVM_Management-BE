using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IVehicleModelRepository : IGenericRepository<VehicleModel>
    {
        Task<IEnumerable<VehicleModel>> GetAllOrderedByCreatedDateDescAsync();
        Task<IEnumerable<VehicleModel>> GetByRankingAsync(Models.Enums.VehicleModelRanking ranking);
        Task<VehicleModel?> UpdateAsync(VehicleModel entity);
        Task<VehicleModel?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<IEnumerable<VehicleModel>> SearchByQueryAsync(string q);
    }
}
