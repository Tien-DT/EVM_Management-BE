using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EVMManagement.API.Services;
using EVMManagement.BLL.DTOs.Request.TransportDetail;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.TransportDetail;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class TransportDetailsController : BaseController
    {
        public TransportDetailsController(IServiceFacade services) : base(services)
        {
        }

        /* Disabled - frontend does not query transport details list
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TransportDetailFilterDto filter)
        {
            filter ??= new TransportDetailFilterDto();

            if (filter.PageNumber < 1 || filter.PageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Giá trị PageNumber và PageSize phải lớn hơn 0", null, 400));
            }

            var result = await Services.TransportDetailService.GetAllAsync(filter);
            return Ok(ApiResponse<PagedResult<TransportDetailResponseDto>>.CreateSuccess(result));
        }
        */

        /* Disabled - frontend does not fetch transport detail by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var detail = await Services.TransportDetailService.GetByIdAsync(id);
            if (detail == null)
            {
                return NotFound(ApiResponse<TransportDetailResponseDto>.CreateFail("Không tìm thấy chi tiết vận chuyển theo mã yêu cầu", null, 404));
            }

            return Ok(ApiResponse<TransportDetailResponseDto>.CreateSuccess(detail));
        }
        */

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] List<TransportDetailCreateDto> dtos)
        {
            if (dtos == null || dtos.Count == 0)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Danh sách chi tiết vận chuyển không được để trống", null, 400));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<List<TransportDetailResponseDto>>.CreateFail("Dữ liệu tạo chi tiết vận chuyển không hợp lệ", errors, 400));
            }

            try
            {
                var created = await Services.TransportDetailService.CreateAsync(dtos);
                return StatusCode(201, ApiResponse<List<TransportDetailResponseDto>>.CreateSuccess(created));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<List<TransportDetailResponseDto>>.CreateFail(ex.Message, null, 400));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<List<TransportDetailResponseDto>>.CreateFail(ex.Message, null, 400));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<List<TransportDetailResponseDto>>.CreateFail(ex.Message, null, 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<TransportDetailResponseDto>>.CreateFail($"Xảy ra lỗi khi tạo chi tiết vận chuyển: {ex.Message}", null, 500));
            }
        }

        /* Disabled - frontend does not update/delete transport details directly
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TransportDetailUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<TransportDetailResponseDto>.CreateFail("Dữ liệu cập nhật chi tiết vận chuyển không hợp lệ", errors, 400));
            }

            try
            {
                var updated = await Services.TransportDetailService.UpdateAsync(id, dto);
                if (updated == null)
                {
                    return NotFound(ApiResponse<TransportDetailResponseDto>.CreateFail("Không tìm thấy chi tiết vận chuyển theo mã yêu cầu", null, 404));
                }

                return Ok(ApiResponse<TransportDetailResponseDto>.CreateSuccess(updated));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<TransportDetailResponseDto>.CreateFail(ex.Message, null, 400));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<TransportDetailResponseDto>.CreateFail(ex.Message, null, 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TransportDetailResponseDto>.CreateFail($"Xảy ra lỗi khi cập nhật chi tiết vận chuyển: {ex.Message}", null, 500));
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await Services.TransportDetailService.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null)
            {
                return NotFound(ApiResponse<TransportDetailResponseDto>.CreateFail("Không tìm thấy chi tiết vận chuyển theo mã yêu cầu", null, 404));
            }

            return Ok(ApiResponse<TransportDetailResponseDto>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await Services.TransportDetailService.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound(ApiResponse<string>.CreateFail("Không tìm thấy chi tiết vận chuyển theo mã yêu cầu", null, 404));
                }

                return Ok(ApiResponse<string>.CreateSuccess("Xóa chi tiết vận chuyển thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.CreateFail($"Xảy ra lỗi khi xóa chi tiết vận chuyển: {ex.Message}", null, 500));
            }
        }
        */
    }
}

