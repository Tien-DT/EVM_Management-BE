using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Vehicle;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IVehicleVariantService
    {
        Task<VehicleVariant> CreateVehicleVariantAsync(VehicleVariantCreateDto dto);
        Task<PagedResult<VehicleVariantResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<VehicleVariantResponse?> GetByIdAsync(Guid id);
        Task<VehicleVariantResponse?> UpdateAsync(Guid id, VehicleVariantUpdateDto dto);
        Task<VehicleVariantResponse?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<bool> DeleteAsync(Guid id);
        Task<PagedResult<VehicleVariantResponse>> GetByModelIdAsync(Guid modelId, int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<VehicleVariantResponse>> GetByDealerIdAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10);
    }
}
