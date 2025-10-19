using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.AvailableSlot;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.AvailableSlot;
using System;
using System.Threading.Tasks;
using System.Linq;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AvailableSlotsController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public AvailableSlotsController(IServiceFacade services)
        {
            _services = services;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AvailableSlotCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AvailableSlotResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            try
            {
                var created = await _services.AvailableSlotService.CreateAvailableSlotAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<AvailableSlotResponseDto>.CreateSuccess(created));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AvailableSlotResponseDto>.CreateFail(ex.Message, null, 400));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.AvailableSlotService.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<AvailableSlotResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.AvailableSlotService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<AvailableSlotResponseDto>.CreateFail("AvailableSlot not found", null, 404));
            return Ok(ApiResponse<AvailableSlotResponseDto>.CreateSuccess(item));
        }

        [HttpGet("by-vehicle/{vehicleId}")]
        public async Task<IActionResult> GetByVehicleId(Guid vehicleId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.AvailableSlotService.GetByVehicleIdAsync(vehicleId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<AvailableSlotResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("by-dealer/{dealerId}")]
        public async Task<IActionResult> GetByDealerId(Guid dealerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.AvailableSlotService.GetByDealerIdAsync(dealerId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<AvailableSlotResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.AvailableSlotService.GetAvailableAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<AvailableSlotResponseDto>>.CreateSuccess(result));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AvailableSlotUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AvailableSlotResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _services.AvailableSlotService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<AvailableSlotResponseDto>.CreateFail("AvailableSlot not found", null, 404));
            return Ok(ApiResponse<AvailableSlotResponseDto>.CreateSuccess(updated));
        }

        [HttpPatch("{id}/is-deleted")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromBody] bool isDeleted)
        {
            var updated = await _services.AvailableSlotService.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<AvailableSlotResponseDto>.CreateFail("AvailableSlot not found", null, 404));
            return Ok(ApiResponse<AvailableSlotResponseDto>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _services.AvailableSlotService.DeleteAsync(id);
            if (!result) return NotFound(ApiResponse<string>.CreateFail("AvailableSlot not found", null, 404));
            return Ok(ApiResponse<string>.CreateSuccess("AvailableSlot deleted successfully"));
        }
    }
}

