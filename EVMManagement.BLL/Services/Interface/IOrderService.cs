using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Order;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Order;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(OrderCreateDto dto);
        Task<PagedResult<OrderResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<OrderResponse?> GetByIdAsync(Guid id);
        Task<OrderResponse?> UpdateAsync(Guid id, OrderUpdateDto dto);
        Task<OrderResponse?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<bool> DeleteAsync(Guid id);
    }
}
