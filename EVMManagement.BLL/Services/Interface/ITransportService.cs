using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Transport;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Transport;

namespace EVMManagement.BLL.Services.Interface
{
    public interface ITransportService
    {
        Task<TransportResponseDto> CreateAsync(TransportCreateDto dto);
        Task<PagedResult<TransportResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<TransportResponseDto?> GetByIdAsync(Guid id);
        Task<TransportResponseDto?> UpdateAsync(Guid id, TransportUpdateDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}

