using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Vehicle;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.API.Services;


namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class VehicleModelsController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public VehicleModelsController(IServiceFacade services)
        {
            _services = services;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleModelCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<VehicleModelResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            try
            {
                var created = await _services.VehicleModelService.CreateVehicleModelAsync(dto);
                return Ok(ApiResponse<VehicleModelResponseDto>.CreateSuccess(created));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<VehicleModelResponseDto>.CreateFail(ex.Message, new List<string> { ex.Message }, 400));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.VehicleModelService.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleModelResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var result = await _services.VehicleModelService.GetByIdAsync(id);
            if (result == null) return NotFound(ApiResponse<VehicleModelResponseDto>.CreateFail("VehicleModel not found", null, 404));
            return Ok(ApiResponse<VehicleModelResponseDto>.CreateSuccess(result));
        }

        [HttpGet("by-ranking")]
        public async Task<IActionResult> GetByRanking([FromQuery] VehicleModelRanking ranking, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.VehicleModelService.GetByRankingAsync(ranking, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleModelResponseDto>>.CreateSuccess(result));
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] VehicleModelUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<VehicleModelResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            try
            {
                var updated = await _services.VehicleModelService.UpdateVehicleModelAsync(id, dto);
                if (updated == null) return NotFound(ApiResponse<VehicleModelResponseDto>.CreateFail("VehicleModel not found", null, 404));
                return Ok(ApiResponse<VehicleModelResponseDto>.CreateSuccess(updated));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<VehicleModelResponseDto>.CreateFail(ex.Message, new List<string> { ex.Message }, 400));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete([FromRoute] Guid id)
        {
            var deleted = await _services.VehicleModelService.SoftDeleteAsync(id);
            if (deleted == null) return NotFound(ApiResponse<VehicleModelResponseDto>.CreateFail("Không tìm thấy mẫu xe.", null, 404));
            return Ok(ApiResponse<VehicleModelResponseDto>.CreateSuccess(deleted, "Đã xóa mềm mẫu xe."));
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var results = await _services.VehicleModelService.SearchByQueryAsync(q, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleModelResponseDto>>.CreateSuccess(results));

        }

        [HttpGet("dealer/{dealerId}/models")]
        public async Task<IActionResult> GetByDealer(Guid dealerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            if (dealerId == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.CreateFail("DealerId is required", null, 400));
            }

            var result = await _services.VehicleModelService.GetByDealerAsync(dealerId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleModelResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("with-stock")]
        public async Task<IActionResult> GetAllWithDealerStock([FromQuery] Guid dealerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            if (dealerId == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.CreateFail("DealerId is required", null, 400));
            }

            var result = await _services.VehicleModelService.GetAllWithDealerStockAsync(dealerId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleModelWithStockResponseDto>>.CreateSuccess(result));
        }
    }
}
