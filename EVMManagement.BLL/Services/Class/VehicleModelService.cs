using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.BLL.Services.Class
{
    using EVMManagement.BLL.DTOs.Response.Vehicle;

    public class VehicleModelService : IVehicleModelService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VehicleModelService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

    public async Task<VehicleModelResponseDto> CreateVehicleModelAsync(VehicleModelCreateDto dto)
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

            return MapToDto(model);
        }

        public async Task<IEnumerable<VehicleModelResponseDto>> GetAllAsync()
        {
            var models = await _unitOfWork.VehicleModels.GetAllOrderedByCreatedDateDescAsync();
            return models.Select(MapToDto);
        }

        public async Task<IEnumerable<VehicleModelResponseDto>> GetByRankingAsync(VehicleModelRanking ranking)
        {
            var models = await _unitOfWork.VehicleModels.GetByRankingAsync(ranking);
            return models.Select(MapToDto);
        }

        

        public async Task<VehicleModelResponseDto?> UpdateVehicleModelAsync(Guid id, VehicleModelUpdateDto dto)
        {
          
            var existing = await _unitOfWork.VehicleModels.GetByIdAsync(id);
            if (existing == null) return null;

            existing.Code = dto.Code;
            existing.Name = dto.Name;
            existing.LaunchDate = dto.LaunchDate;
            existing.Description = dto.Description;
            existing.Status = dto.Status;
            existing.Ranking = dto.Ranking;
            var updated = await _unitOfWork.VehicleModels.UpdateAsync(existing);
            if (updated == null) return null;
            return MapToDto(updated);
        }

        private VehicleModelResponseDto MapToDto(VehicleModel model)
        {
            return new VehicleModelResponseDto
            {
                Id = model.Id,
                Code = model.Code,
                Name = model.Name,
                LaunchDate = model.LaunchDate,
                Description = model.Description,
                Status = model.Status,
                Ranking = model.Ranking,
                CreatedDate = model.CreatedDate,
                ModifiedDate = model.ModifiedDate
            };
        }
    }
}
