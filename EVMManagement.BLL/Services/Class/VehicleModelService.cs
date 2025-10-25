using AutoMapper;
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
        private readonly IMapper _mapper;

        public VehicleModelService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

    public async Task<VehicleModelResponseDto> CreateVehicleModelAsync(VehicleModelCreateDto dto)
        {
            var model = _mapper.Map<VehicleModel>(dto);

            await _unitOfWork.VehicleModels.AddAsync(model);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<VehicleModelResponseDto>(model);
        }

        public async Task<PagedResult<VehicleModelResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.VehicleModels.GetQueryable();

            var totalCount = await query.CountAsync(x => !x.IsDeleted);

            var items = await query
                .Where(x => !x.IsDeleted)
                .OrderByDescending(u => u.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = _mapper.Map<List<VehicleModelResponseDto>>(items);

            return PagedResult<VehicleModelResponseDto>.Create(responses, totalCount, pageNumber, pageSize);
        }

        public async Task<VehicleModelResponseDto?> GetByIdAsync(Guid id)
        {
            var model = await _unitOfWork.VehicleModels.GetByIdAsync(id);
            if (model == null) return null;
            return _mapper.Map<VehicleModelResponseDto>(model);
        }

        public async Task<PagedResult<VehicleModelResponseDto>> GetByRankingAsync(VehicleModelRanking ranking, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.VehicleModels.GetByRankingAsync(ranking);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = _mapper.Map<List<VehicleModelResponseDto>>(items);

            return PagedResult<VehicleModelResponseDto>.Create(responses, totalCount, pageNumber, pageSize);
        }

        

        public async Task<VehicleModelResponseDto?> UpdateVehicleModelAsync(Guid id, VehicleModelUpdateDto dto)
        {
          
            var existing = await _unitOfWork.VehicleModels.GetByIdAsync(id);
            if (existing == null) return null;

            _mapper.Map(dto, existing);
            _unitOfWork.VehicleModels.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<VehicleModelResponseDto>(existing);
        }

        public async Task<VehicleModelResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var existing = await _unitOfWork.VehicleModels.GetByIdAsync(id);
            if (existing == null) return null;

            existing.IsDeleted = isDeleted;
            existing.DeletedDate = isDeleted ? DateTime.UtcNow : null;
            _unitOfWork.VehicleModels.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<VehicleModelResponseDto>(existing);
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

            var responses = _mapper.Map<List<VehicleModelResponseDto>>(items);

            return PagedResult<VehicleModelResponseDto>.Create(responses, totalCount, pageNumber, pageSize);
            
        }


    }
}
