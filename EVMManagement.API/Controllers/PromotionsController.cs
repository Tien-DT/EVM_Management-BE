using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.Promotion;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Promotion;
using System;
using System.Threading.Tasks;
using System.Linq;
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
                return BadRequest(ApiResponse<PromotionResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _services.PromotionService.CreatePromotionAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<PromotionResponseDto>.CreateSuccess(created));
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PromotionUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<PromotionResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _services.PromotionService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<PromotionResponseDto>.CreateFail("Promotion not found", null, 404));
            return Ok(ApiResponse<PromotionResponseDto>.CreateSuccess(updated));
        }

        [HttpPatch("{id}/is-active")]
        public async Task<IActionResult> UpdateIsActive(Guid id, [FromQuery] bool isActive)
        {
            var updated = await _services.PromotionService.UpdateIsActiveAsync(id, isActive);
            if (updated == null) return NotFound(ApiResponse<PromotionResponseDto>.CreateFail("Promotion not found", null, 404));
            return Ok(ApiResponse<PromotionResponseDto>.CreateSuccess(updated));
        }

        [HttpPatch("{id}/is-deleted")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await _services.PromotionService.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<PromotionResponseDto>.CreateFail("Promotion not found", null, 404));
            return Ok(ApiResponse<PromotionResponseDto>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _services.PromotionService.DeleteAsync(id);
            if (!deleted) return NotFound(ApiResponse<string>.CreateFail("Promotion not found", null, 404));
            return Ok(ApiResponse<string>.CreateSuccess("Deleted"));
        }
    }
}

