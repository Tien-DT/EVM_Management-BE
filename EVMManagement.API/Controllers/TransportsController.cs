using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EVMManagement.API.Services;
using EVMManagement.BLL.DTOs.Request.Transport;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Transport;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class TransportsController : BaseController
    {
        private readonly IServiceFacade _services;

        public TransportsController(IServiceFacade services) : base(services)
        {
            _services = services;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TransportFilterDto filter)
        {
            filter ??= new TransportFilterDto();

            if (filter.PageNumber < 1 || filter.PageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Giá trị PageNumber và PageSize phải lớn hơn 0", null, 400));
            }

            var result = await _services.TransportService.GetAllAsync(filter);
            return Ok(ApiResponse<PagedResult<TransportResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("dealer/{dealerId}")]
        public async Task<IActionResult> GetByDealer(Guid dealerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Giá trị PageNumber và PageSize phải lớn hơn 0", null, 400));
            }

            var result = await _services.TransportService.GetByDealerAsync(dealerId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<TransportResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetByOrder(Guid orderId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Giá trị PageNumber và PageSize phải lớn hơn 0", null, 400));
            }

            var result = await _services.TransportService.GetByOrderAsync(orderId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<TransportResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _services.TransportService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse<TransportResponseDto>.CreateFail("Không tìm thấy vận chuyển với mã yêu cầu", null, 404));
            }

            return Ok(ApiResponse<TransportResponseDto>.CreateSuccess(result));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TransportCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<TransportResponseDto>.CreateFail("Dữ liệu tạo vận chuyển không hợp lệ", errors, 400));
            }

            try
            {
                var created = await _services.TransportService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<TransportResponseDto>.CreateSuccess(created));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<TransportResponseDto>.CreateFail(ex.Message, null, 400));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<TransportResponseDto>.CreateFail(ex.Message, null, 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TransportResponseDto>.CreateFail($"Xảy ra lỗi khi tạo vận chuyển: {ex.Message}", null, 500));
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            try
            {
                var result = await _services.TransportService.CancelAsync(id);
                return Ok(ApiResponse<TransportResponseDto>.CreateSuccess(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<TransportResponseDto>.CreateFail(ex.Message, null, 404));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<TransportResponseDto>.CreateFail(ex.Message, null, 400));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TransportResponseDto>.CreateFail($"Xảy ra lỗi khi hủy vận chuyển: {ex.Message}", null, 500));
            }
        }

        [HttpPost("{id}/handover/confirm")]
        public async Task<IActionResult> ConfirmHandover(Guid id)
        {
            try
            {
                var result = await _services.TransportService.ConfirmHandoverAsync(id);
                return Ok(ApiResponse<TransportResponseDto>.CreateSuccess(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<TransportResponseDto>.CreateFail(ex.Message, null, 404));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<TransportResponseDto>.CreateFail(ex.Message, null, 400));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TransportResponseDto>.CreateFail($"Xảy ra lỗi khi xác nhận bàn giao: {ex.Message}", null, 500));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TransportUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<TransportResponseDto>.CreateFail("Dữ liệu cập nhật vận chuyển không hợp lệ", errors, 400));
            }

            try
            {
                var updated = await _services.TransportService.UpdateAsync(id, dto);
                if (updated == null)
                {
                    return NotFound(ApiResponse<TransportResponseDto>.CreateFail("Không tìm thấy vận chuyển với mã yêu cầu", null, 404));
                }

                return Ok(ApiResponse<TransportResponseDto>.CreateSuccess(updated));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<TransportResponseDto>.CreateFail(ex.Message, null, 400));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<TransportResponseDto>.CreateFail(ex.Message, null, 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TransportResponseDto>.CreateFail($"Xảy ra lỗi khi cập nhật vận chuyển: {ex.Message}", null, 500));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _services.TransportService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(ApiResponse<string>.CreateFail("Không tìm thấy vận chuyển với mã yêu cầu", null, 404));
                }

                return Ok(ApiResponse<string>.CreateSuccess("Xóa vận chuyển thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.CreateFail($"Xảy ra lỗi khi xóa vận chuyển: {ex.Message}", null, 500));
            }
        }
    }
}
