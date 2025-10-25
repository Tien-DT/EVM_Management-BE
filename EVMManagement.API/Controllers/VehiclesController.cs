using EVMManagement.API.Services;
using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Vehicle;
using EVMManagement.DAL.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class VehiclesController :  BaseController
    {
        private readonly IServiceFacade _services;

        public VehiclesController(IServiceFacade services) : base(services)
        {
            _services = services;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.VehicleService.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("details")]
        public async Task<IActionResult> GetAllWithDetails([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.VehicleService.GetAllWithDetailsAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleDetailResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.VehicleService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<VehicleResponseDto>.CreateFail("Vehicle not found", null, 404));
            return Ok(ApiResponse<VehicleResponseDto>.CreateSuccess(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<VehicleResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _services.VehicleService.CreateVehicleAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<VehicleResponseDto>.CreateSuccess(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] VehicleUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<VehicleResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _services.VehicleService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<VehicleResponseDto>.CreateFail("Vehicle not found", null, 404));
            return Ok(ApiResponse<VehicleResponseDto>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> UpdateIsDeleted([FromRoute] Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await _services.VehicleService.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<VehicleResponseDto>.CreateFail("Vehicle not found", null, 404));
            return Ok(ApiResponse<VehicleResponseDto>.CreateSuccess(updated));
        }

      

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var results = await _services.VehicleService.SearchByQueryAsync(q, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleResponseDto>>.CreateSuccess(results));

        }



        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] EVMManagement.BLL.DTOs.Request.Vehicle.VehicleFilterDto filter)
        {
            if (filter.PageNumber < 1 || filter.PageSize < 1)
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));

            var results = await _services.VehicleService.GetByFilterAsync(filter);
            return Ok(ApiResponse<PagedResult<VehicleResponseDto>>.CreateSuccess(results));
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromQuery] VehicleStatus status)
        {
            var updated = await _services.VehicleService.UpdateStatusAsync(id, status);
            if (updated == null) return NotFound(ApiResponse<VehicleResponseDto>.CreateFail("Vehicle not found", null, 404));
            return Ok(ApiResponse<VehicleResponseDto>.CreateSuccess(updated));
        }

        [HttpGet("check-stock")]
        public async Task<IActionResult> CheckStock([FromQuery] Guid variantId, [FromQuery] Guid dealerId, [FromQuery] int quantity)
        {
            if (variantId == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.CreateFail("VariantId is required", null, 400));
            }

            if (dealerId == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.CreateFail("DealerId is required", null, 400));
            }

            if (quantity < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Quantity must be greater than 0", null, 400));
            }

            var result = await _services.VehicleService.CheckStockAvailabilityAsync(variantId, dealerId, quantity);
            return Ok(ApiResponse<StockCheckResponseDto>.CreateSuccess(result));
        }

        [HttpGet("dealer/{dealerId}/models")]
        [Authorize(Roles = "DEALER_MANAGER,DEALER_STAFF")]
        public async Task<IActionResult> GetModelsByDealer(Guid dealerId)
        {
            var currentRole = GetCurrentRole();
            if (!currentRole.HasValue)
            {
                return Unauthorized(ApiResponse<string>.CreateFail("Không tìm thấy thông tin role của tài khoản.", errorCode: 401));
            }
            if (dealerId == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.CreateFail("DealerId is required", null, 400));
            }

            var result = await _services.VehicleService.GetModelsByDealerAsync(dealerId);
            return Ok(ApiResponse<List<DealerModelListDto>>.CreateSuccess(result));
        }

        [HttpGet("dealer/{dealerId}/models/{modelId}/variants")]
        [Authorize(Roles = "DEALER_MANAGER,DEALER_STAFF")]
        public async Task<IActionResult> GetVariantsByDealerAndModel(Guid dealerId, Guid modelId)
        {
            var currentRole = GetCurrentRole();
            if (!currentRole.HasValue)
            {
                return Unauthorized(ApiResponse<string>.CreateFail("Không tìm thấy thông tin role của tài khoản.", errorCode: 401));
            }
            if (dealerId == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.CreateFail("DealerId is required", null, 400));
            }

            if (modelId == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.CreateFail("ModelId is required", null, 400));
            }

            var result = await _services.VehicleService.GetVariantsByDealerAndModelAsync(dealerId, modelId);
            return Ok(ApiResponse<List<DealerVariantListDto>>.CreateSuccess(result));
        }
    }
}
