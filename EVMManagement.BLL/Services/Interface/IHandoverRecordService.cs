using System;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.HandoverRecord;
using EVMManagement.BLL.DTOs.Request.Order;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.HandoverRecord;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IHandoverRecordService
    {
        Task<HandoverRecordResponseDto> CreateAsync(HandoverRecordCreateDto dto);
        Task<PagedResult<HandoverRecordResponseDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<HandoverRecordResponseDto?> GetByIdAsync(Guid id);
        Task<HandoverRecordResponseDto?> UpdateAsync(Guid id, HandoverRecordUpdateDto dto);
        Task<HandoverRecordResponseDto?> UpdateIsDeletedAsync(Guid id, bool isDeleted);
        Task<HandoverRecordResponseDto> CreateHandoverWithVehicleAssignmentAsync(Guid orderId, OrderHandoverRequestDto dto);
    }
}
