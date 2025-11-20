using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.BLL.DTOs.Request.Dealer;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Dealer;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class DealersController : ControllerBase
    {
        private readonly IDealerService _service;

        public DealersController(IDealerService service)
        {
            _service = service;
        }

        // lấy ds dealer với phân trang và lọc
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _service.GetAllAsync(pageNumber, pageSize, search, isActive);
            return Ok(ApiResponse<PagedResult<DealerResponseDto>>.CreateSuccess(result));
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<DealerResponseDto>.CreateFail("Dealer not found", null, 404));
            return Ok(ApiResponse<DealerResponseDto>.CreateSuccess(item));
        }

        // tạo mới dealer (chỉ EVM admin)
        [HttpPost]
        [Authorize(Roles = "EVM_ADMIN")]
        public async Task<IActionResult> Create([FromBody] CreateDealerDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<Dealer>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _service.CreateDealerAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<Dealer>.CreateSuccess(created));
        }

        // cập nhật dealer (chỉ EVM admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "EVM_ADMIN")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDealerDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<DealerResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<DealerResponseDto>.CreateFail("Dealer not found", null, 404));
            return Ok(ApiResponse<DealerResponseDto>.CreateSuccess(updated));
        }

        // xóa dealer (chỉ EVM admin) - soft delete
        /* Disabled - frontend uses DELETE instead of patching isDeleted
        [HttpPatch("{id}")]
        [Authorize(Roles = "EVM_ADMIN")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await _service.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<DealerResponseDto>.CreateFail("Dealer not found", null, 404));
            return Ok(ApiResponse<DealerResponseDto>.CreateSuccess(updated));
        }
        */

        // hard delete
        [HttpDelete("{id}")]
        [Authorize(Roles = "EVM_ADMIN")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound(ApiResponse<string>.CreateFail("Dealer not found", null, 404));
            return Ok(ApiResponse<string>.CreateSuccess("Deleted"));
        }
    }
}

