using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.BLL.Services.Class
{
    using EVMManagement.BLL.DTOs.Response.Vehicle;
    using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        public async Task<PagedResult<VehicleModelResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.VehicleModels.GetQueryable();

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(u => u.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = items.Select(MapToDto).ToList();

            return PagedResult<VehicleModelResponseDto>.Create(responses, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<VehicleModelResponseDto>> GetByRankingAsync(VehicleModelRanking ranking, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.VehicleModels.GetByRankingAsync(ranking);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = items.Select(MapToDto).ToList();

            return PagedResult<VehicleModelResponseDto>.Create(responses, totalCount, pageNumber, pageSize);
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
            _unitOfWork.VehicleModels.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(existing);
        }

        public async Task<VehicleModelResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var existing = await _unitOfWork.VehicleModels.GetByIdAsync(id);
            if (existing == null) return null;

            existing.IsDeleted = isDeleted;
            existing.DeletedDate = isDeleted ? DateTime.UtcNow : null;
            _unitOfWork.VehicleModels.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(existing);
        }


        public async Task<PagedResult<VehicleModelResponseDto>> SearchByQueryAsync(string? q, int pageNumber = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                GetAllAsync();
            }
            var query = _unitOfWork.VehicleModels.SearchByQueryAsync(q!);
            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = items.Select(MapToDto).ToList();

            return PagedResult<VehicleModelResponseDto>.Create(responses, totalCount, pageNumber, pageSize);
            
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
                ModifiedDate = model.ModifiedDate,
                DeletedDate = model.DeletedDate,
                IsDeleted = model.IsDeleted
            };
        }
    }
}
