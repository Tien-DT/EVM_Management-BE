using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Promotion;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Promotion;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IPromotionService
    {
        Task<PromotionResponseDto> CreatePromotionAsync(PromotionCreateDto dto);
        Task<PagedResult<PromotionResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<PromotionResponseDto?> GetByIdAsync(Guid id);
        Task<PagedResult<PromotionResponseDto>> SearchAsync(string? query, int pageNumber = 1, int pageSize = 10);
        Task<PromotionResponseDto?> UpdateAsync(Guid id, PromotionUpdateDto dto);
        Task<PromotionResponseDto?> UpdateIsActiveAsync(Guid id, bool isActive);
    }
}

