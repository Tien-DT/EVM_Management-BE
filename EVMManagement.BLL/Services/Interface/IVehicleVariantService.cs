using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IVehicleVariantService
    {
        Task<VehicleVariant> CreateVehicleVariantAsync(VehicleVariantCreateDto dto);
    }
}
