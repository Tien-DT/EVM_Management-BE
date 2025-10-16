using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Promotion;
using EVMManagement.BLL.DTOs.Response.Promotion;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IPromotionService
    {
        Task<PromotionResponseDto> CreatePromotionAsync(PromotionCreateDto dto);
    }
}

