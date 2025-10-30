using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.OrderDetail;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.OrderDetail;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IOrderDetailService
    {
        Task<OrderDetail> CreateOrderDetailAsync(OrderDetailCreateDto dto);
        Task<List<OrderDetail>> CreateOrderDetailsAsync(List<OrderDetailCreateDto> dtos);
        Task<OrderDetailBulkCreateResponse> CreateOrderDetailsV2Async(List<OrderDetailCreateDto> dtos);
        Task<PagedResult<OrderDetailResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<OrderDetailResponse?> GetByIdAsync(Guid id);
        Task<OrderDetailResponse?> UpdateAsync(Guid id, OrderDetailUpdateDto dto);
        Task<OrderDetailResponse?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<bool> DeleteAsync(Guid id);
    }
}
