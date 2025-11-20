using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.Promotion;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Promotion;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PromotionsController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public PromotionsController(IServiceFacade services)
        {
            _services = services;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PromotionCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<PromotionResponseDto>.CreateFail("Dữ liệu không hợp lệ.", errors, 400));
            }

            try
            {
                var created = await _services.PromotionService.CreatePromotionAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<PromotionResponseDto>.CreateSuccess(created));
            }
            catch (ArgumentException ex)
            {
                var errors = new List<string> { ex.Message };
                return BadRequest(ApiResponse<PromotionResponseDto>.CreateFail("Dữ liệu không hợp lệ.", errors, 400));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.PromotionService.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<PromotionResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.PromotionService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<PromotionResponseDto>.CreateFail("Promotion not found", null, 404));
            return Ok(ApiResponse<PromotionResponseDto>.CreateSuccess(item));
        }

        [HttpGet("vehicle-promotions")]
        public async Task<IActionResult> GetVehiclePromotions([FromQuery] Guid? variantId, [FromQuery] Guid? promotionId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber và PageSize phải lớn hơn 0", null, 400));
            }

            if (!variantId.HasValue && !promotionId.HasValue)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Cần cung cấp VariantId hoặc PromotionId", null, 400));
            }

            try
            {
                var result = await _services.PromotionService.GetVehiclePromotionsAsync(variantId, promotionId, pageNumber, pageSize);
                return Ok(ApiResponse<PagedResult<VehiclePromotionResponseDto>>.CreateSuccess(result));
            }
            catch (ArgumentException ex)
            {
                var errors = new List<string> { ex.Message };
                return BadRequest(ApiResponse<VehiclePromotionResponseDto>.CreateFail("Dữ liệu không hợp lệ.", errors, 400));
            }
        }

        /* Disabled - frontend not using promotion search endpoint
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var results = await _services.PromotionService.SearchAsync(q, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<PromotionResponseDto>>.CreateSuccess(results));
        }
        */

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PromotionUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<PromotionResponseDto>.CreateFail("Dữ liệu không hợp lệ.", errors, 400));
            }

            try
            {
                var updated = await _services.PromotionService.UpdateAsync(id, dto);
                if (updated == null) return NotFound(ApiResponse<PromotionResponseDto>.CreateFail("Promotion not found", null, 404));
                return Ok(ApiResponse<PromotionResponseDto>.CreateSuccess(updated));
            }
            catch (ArgumentException ex)
            {
                var errors = new List<string> { ex.Message };
                return BadRequest(ApiResponse<PromotionResponseDto>.CreateFail("Dữ liệu không hợp lệ.", errors, 400));
            }
        }

        /* Disabled - frontend does not toggle promotion active status via API
        [HttpPatch("{id}/is-active")]
        public async Task<IActionResult> UpdateIsActive(Guid id, [FromQuery] bool isActive)
        {
            var updated = await _services.PromotionService.UpdateIsActiveAsync(id, isActive);
            if (updated == null) return NotFound(ApiResponse<PromotionResponseDto>.CreateFail("Promotion not found", null, 404));
            return Ok(ApiResponse<PromotionResponseDto>.CreateSuccess(updated));
        }
        */

        /* Disabled - frontend uses DELETE instead of soft delete
        [HttpPatch("{id}/is-deleted")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await _services.PromotionService.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<PromotionResponseDto>.CreateFail("Promotion not found", null, 404));
            return Ok(ApiResponse<PromotionResponseDto>.CreateSuccess(updated));
        }
        */

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _services.PromotionService.DeleteAsync(id);
            if (!deleted) return NotFound(ApiResponse<string>.CreateFail("Promotion not found", null, 404));
            return Ok(ApiResponse<string>.CreateSuccess("Deleted"));
        }
    }
}

