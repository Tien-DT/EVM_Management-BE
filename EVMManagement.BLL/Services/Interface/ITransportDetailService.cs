using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.TransportDetail;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.TransportDetail;

namespace EVMManagement.BLL.Services.Interface
{
    public interface ITransportDetailService
    {
        Task<List<TransportDetailResponseDto>> CreateAsync(List<TransportDetailCreateDto> dtos);
        Task<PagedResult<TransportDetailResponseDto>> GetAllAsync(TransportDetailFilterDto filter);
        Task<TransportDetailResponseDto?> GetByIdAsync(Guid id);
        Task<TransportDetailResponseDto?> UpdateAsync(Guid id, TransportDetailUpdateDto dto);
        Task<TransportDetailResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<bool> DeleteAsync(Guid id);
    }
}

