using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Vehicle;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

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

        public async Task<PagedResult<VehicleVariantResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.VehicleVariants.GetQueryable();
            var totalCount = await _unitOfWork.VehicleVariants.CountAsync(x => !x.IsDeleted);
            
            var items = await query
                .Where(v => !v.IsDeleted)
                .Include(v => v.VehicleModel)
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new VehicleVariantResponse
                {
                    Id = x.Id,
                    ModelId = x.ModelId,
                    ModelName = x.VehicleModel!.Name,
                    Color = x.Color,
                    ChargingTime = x.ChargingTime,
                    Engine = x.Engine,
                    Capacity = x.Capacity,
                    ShockAbsorbers = x.ShockAbsorbers,
                    BatteryType = x.BatteryType,
                    BatteryLife = x.BatteryLife,
                    MaximumSpeed = x.MaximumSpeed,
                    DistancePerCharge = x.DistancePerCharge,
                    Weight = x.Weight,
                    GroundClearance = x.GroundClearance,
                    Brakes = x.Brakes,
                    Length = x.Length,
                    Width = x.Width,
                    Height = x.Height,
                    Price = x.Price,
                    TrunkWidth = x.TrunkWidth,
                    Description = x.Description,
                    ChargingCapacity = x.ChargingCapacity,
                    ImageUrl = x.ImageUrl,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    IsDeleted = x.IsDeleted
                })
                .ToListAsync();

            return PagedResult<VehicleVariantResponse>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<VehicleVariantResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _unitOfWork.VehicleVariants.GetByIdAsync(id);
            if (entity == null) return null;

            var modelName = entity.VehicleModel?.Name ?? "Unknown";

            return new VehicleVariantResponse
            {
                Id = entity.Id,
                ModelId = entity.ModelId,
                ModelName = modelName,
                Color = entity.Color,
                ChargingTime = entity.ChargingTime,
                Engine = entity.Engine,
                Capacity = entity.Capacity,
                ShockAbsorbers = entity.ShockAbsorbers,
                BatteryType = entity.BatteryType,
                BatteryLife = entity.BatteryLife,
                MaximumSpeed = entity.MaximumSpeed,
                DistancePerCharge = entity.DistancePerCharge,
                Weight = entity.Weight,
                GroundClearance = entity.GroundClearance,
                Brakes = entity.Brakes,
                Length = entity.Length,
                Width = entity.Width,
                Height = entity.Height,
                Price = entity.Price,
                TrunkWidth = entity.TrunkWidth,
                Description = entity.Description,
                ChargingCapacity = entity.ChargingCapacity,
                ImageUrl = entity.ImageUrl,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.ModifiedDate,
                IsDeleted = entity.IsDeleted
            };
        }

        public async Task<VehicleVariantResponse?> UpdateAsync(Guid id, VehicleVariantUpdateDto dto)
        {
            var entity = await _unitOfWork.VehicleVariants.GetByIdAsync(id);
            if (entity == null) return null;

            if (dto.ModelId.HasValue) entity.ModelId = dto.ModelId.Value;
            if (dto.Color != null) entity.Color = dto.Color;
            if (dto.ChargingTime.HasValue) entity.ChargingTime = dto.ChargingTime;
            if (dto.Engine != null) entity.Engine = dto.Engine;
            if (dto.Capacity.HasValue) entity.Capacity = dto.Capacity;
            if (dto.ShockAbsorbers != null) entity.ShockAbsorbers = dto.ShockAbsorbers;
            if (dto.BatteryType != null) entity.BatteryType = dto.BatteryType;
            if (dto.BatteryLife != null) entity.BatteryLife = dto.BatteryLife;
            if (dto.MaximumSpeed.HasValue) entity.MaximumSpeed = dto.MaximumSpeed;
            if (dto.DistancePerCharge != null) entity.DistancePerCharge = dto.DistancePerCharge;
            if (dto.Weight.HasValue) entity.Weight = dto.Weight;
            if (dto.GroundClearance.HasValue) entity.GroundClearance = dto.GroundClearance;
            if (dto.Brakes != null) entity.Brakes = dto.Brakes;
            if (dto.Length.HasValue) entity.Length = dto.Length;
            if (dto.Width.HasValue) entity.Width = dto.Width;
            if (dto.Height.HasValue) entity.Height = dto.Height;
            if (dto.Price.HasValue) entity.Price = dto.Price;
            if (dto.TrunkWidth.HasValue) entity.TrunkWidth = dto.TrunkWidth;
            if (dto.Description != null) entity.Description = dto.Description;
            if (dto.ChargingCapacity.HasValue) entity.ChargingCapacity = dto.ChargingCapacity;
            if (dto.ImageUrl != null) entity.ImageUrl = dto.ImageUrl;

            _unitOfWork.VehicleVariants.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<VehicleVariantResponse?> UpdateIsDeletedAsync(Guid id, bool isDeleted)
        {
            var entity = await _unitOfWork.VehicleVariants.GetByIdAsync(id);
            if (entity == null) return null;

            entity.IsDeleted = isDeleted;
            _unitOfWork.VehicleVariants.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.VehicleVariants.GetByIdAsync(id);
            if (entity == null) return false;

            _unitOfWork.VehicleVariants.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

       
        public async Task<PagedResult<VehicleVariantResponse>> GetByModelIdAsync(Guid modelId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.VehicleVariants.GetQueryable()
                .Where(v => v.ModelId == modelId && !v.IsDeleted)
                .Include(v => v.VehicleModel);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new VehicleVariantResponse
                {
                    Id = x.Id,
                    ModelId = x.ModelId,
                    ModelName = x.VehicleModel!.Name,
                    Color = x.Color,
                    ChargingTime = x.ChargingTime,
                    Engine = x.Engine,
                    Capacity = x.Capacity,
                    ShockAbsorbers = x.ShockAbsorbers,
                    BatteryType = x.BatteryType,
                    BatteryLife = x.BatteryLife,
                    MaximumSpeed = x.MaximumSpeed,
                    DistancePerCharge = x.DistancePerCharge,
                    Weight = x.Weight,
                    GroundClearance = x.GroundClearance,
                    Brakes = x.Brakes,
                    Length = x.Length,
                    Width = x.Width,
                    Height = x.Height,
                    Price = x.Price,
                    TrunkWidth = x.TrunkWidth,
                    Description = x.Description,
                    ChargingCapacity = x.ChargingCapacity,
                    ImageUrl = x.ImageUrl,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    IsDeleted = x.IsDeleted
                })
                .ToListAsync();

            return PagedResult<VehicleVariantResponse>.Create(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<VehicleVariantResponse>> GetByDealerIdAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Vehicles.GetQueryable()
                .Where(v => !v.IsDeleted && v.Warehouse.DealerId == dealerId && v.Status == DAL.Models.Enums.VehicleStatus.IN_STOCK)
                .Select(v => v.VariantId)
                .Distinct();

            var variantIds = await query.ToListAsync();

            var variantsQuery = _unitOfWork.VehicleVariants.GetQueryable()
                .Where(vv => !vv.IsDeleted && variantIds.Contains(vv.Id))
                .Include(vv => vv.VehicleModel);

            var totalCount = await variantsQuery.CountAsync();

            var items = await variantsQuery
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new VehicleVariantResponse
                {
                    Id = x.Id,
                    ModelId = x.ModelId,
                    ModelName = x.VehicleModel!.Name,
                    Color = x.Color,
                    ChargingTime = x.ChargingTime,
                    Engine = x.Engine,
                    Capacity = x.Capacity,
                    ShockAbsorbers = x.ShockAbsorbers,
                    BatteryType = x.BatteryType,
                    BatteryLife = x.BatteryLife,
                    MaximumSpeed = x.MaximumSpeed,
                    DistancePerCharge = x.DistancePerCharge,
                    Weight = x.Weight,
                    GroundClearance = x.GroundClearance,
                    Brakes = x.Brakes,
                    Length = x.Length,
                    Width = x.Width,
                    Height = x.Height,
                    Price = x.Price,
                    TrunkWidth = x.TrunkWidth,
                    Description = x.Description,
                    ChargingCapacity = x.ChargingCapacity,
                    ImageUrl = x.ImageUrl,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    IsDeleted = x.IsDeleted
                })
                .ToListAsync();

            return PagedResult<VehicleVariantResponse>.Create(items, totalCount, pageNumber, pageSize);
        }
    }
}
