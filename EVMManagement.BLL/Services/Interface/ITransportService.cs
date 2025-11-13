using System;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Transport;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Transport;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.BLL.Services.Interface
{
    public interface ITransportService
    {
        Task<TransportResponseDto> CreateAsync(TransportCreateDto dto);
        Task<PagedResult<TransportResponseDto>> GetAllAsync(TransportFilterDto? filter = null);
        Task<PagedResult<TransportResponseDto>> GetByDealerAsync(Guid dealerId, int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<TransportResponseDto>> GetByOrderAsync(Guid orderId, int pageNumber = 1, int pageSize = 10);
        Task<TransportResponseDto?> GetByIdAsync(Guid id);
        Task<TransportResponseDto?> UpdateAsync(Guid id, TransportUpdateDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<TransportResponseDto> CancelAsync(Guid id);
        Task<TransportResponseDto> ConfirmHandoverAsync(Guid transportId);
        Task<TransportResponseDto> AddTransportToWarehouseAsync(AddTransportToWarehouseDto dto);
        IQueryable<Transport> GetQueryableForOData();
    }
}
