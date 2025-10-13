using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Vehicle;

using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IVehicleModelService
    {
        Task<VehicleModel> CreateVehicleModelAsync(VehicleModelCreateDto dto);
        Task<IEnumerable<VehicleModel>> GetAllAsync();
    }
}
