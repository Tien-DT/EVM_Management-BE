using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.Services.Class
{
    public class VehicleModelService : IVehicleModelService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VehicleModelService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<VehicleModel> CreateVehicleModelAsync(VehicleModelCreateDto dto)
        {
            var model = new VehicleModel
            {
                Code = dto.Code,
                Name = dto.Name,
                LaunchDate = dto.LaunchDate,
                Description = dto.Description,
                Status = dto.Status ?? true,
                Ranking = dto.Ranking
            };

            await _unitOfWork.VehicleModels.AddAsync(model);
            await _unitOfWork.SaveChangesAsync();

            return model;
        }

        public async Task<IEnumerable<VehicleModel>> GetAllAsync()
        {
            return await _unitOfWork.VehicleModels.GetAllOrderedByCreatedDateDescAsync();
        }
    }
}
