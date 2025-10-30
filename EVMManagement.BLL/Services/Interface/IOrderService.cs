using System;
using System.Linq;
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
        Task<OrderWithDetailsResponse> CreateOrderWithDetailsAsync(OrderWithDetailsCreateDto dto);
        Task<PagedResult<OrderResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<OrderResponse>> GetByFilterAsync(OrderFilterDto filter);
        Task<OrderResponse?> GetByIdAsync(Guid id);
        Task<OrderWithDetailsResponse?> GetByIdWithDetailsAsync(Guid id);
        Task<OrderResponse?> UpdateAsync(Guid id, OrderUpdateDto dto);
        Task<OrderResponse?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<bool> DeleteAsync(Guid id);
        Task<OrderResponse> CancelOrderAsync(Guid orderId);
        
        Task<OrderFlowResponseDto> RequestDealerManagerApprovalAsync(Guid orderId, DealerManagerApprovalRequestDto dto);
        Task<OrderFlowResponseDto> ApproveDealerOrderRequestAsync(Guid orderId, Guid approvedByUserId);
        Task<bool> NotifyCustomerAsync(Guid orderId, CustomerNotificationRequestDto dto);
        Task<OrderResponse> UpdateCustomerConfirmationAsync(Guid orderId, CustomerConfirmationRequestDto dto);
        Task<OrderFlowResponseDto> ConfirmPaymentAsync(Guid orderId, ConfirmPaymentRequestDto dto);
        IQueryable<Order> GetQueryableForOData();
    }
}
