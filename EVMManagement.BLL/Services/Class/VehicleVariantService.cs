
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;

namespace EVMManagement.BLL.Services.Class
{
    public class VehicleVariantService : IVehicleVariantService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VehicleVariantService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<VehicleVariant> CreateVehicleVariantAsync(VehicleVariantCreateDto dto)
        {
            var vehicleVariant = new VehicleVariant
            {
                ModelId = dto.ModelId,
                Color = dto.Color,
                ChargingTime = dto.ChargingTime,
                Engine = dto.Engine,
                Capacity = dto.Capacity,
                ShockAbsorbers = dto.ShockAbsorbers,
                BatteryType = dto.BatteryType,
                BatteryLife = dto.BatteryLife,
                MaximumSpeed = dto.MaximumSpeed,
                DistancePerCharge = dto.DistancePerCharge,
                Weight = dto.Weight,
                GroundClearance = dto.GroundClearance,
                Brakes = dto.Brakes,
                Length = dto.Length,
                Width = dto.Width,
                Height = dto.Height,
                Price = dto.Price,
                TrunkWidth = dto.TrunkWidth,
                Description = dto.Description,
                ChargingCapacity = dto.ChargingCapacity,
                ImageUrl = dto.ImageUrl
            };

            await _unitOfWork.VehicleVariants.AddAsync(vehicleVariant);
            await _unitOfWork.SaveChangesAsync();

            return vehicleVariant;
        }
    }
}
