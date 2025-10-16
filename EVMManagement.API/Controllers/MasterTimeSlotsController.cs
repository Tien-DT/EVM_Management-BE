using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.BLL.DTOs.Request.MasterTimeSlot;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.MasterTimeSlot;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MasterTimeSlotsController : ControllerBase
    {
        private readonly IMasterTimeSlotService _service;

        public MasterTimeSlotsController(IMasterTimeSlotService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MasterTimeSlotCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<MasterTimeSlotResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _service.CreateMasterTimeSlotAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<MasterTimeSlotResponseDto>.CreateSuccess(created));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _service.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<MasterTimeSlotResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<MasterTimeSlotResponseDto>.CreateFail("MasterTimeSlot not found", null, 404));
            return Ok(ApiResponse<MasterTimeSlotResponseDto>.CreateSuccess(item));
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _service.GetActiveAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<MasterTimeSlotResponseDto>>.CreateSuccess(result));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MasterTimeSlotUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<MasterTimeSlotResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<MasterTimeSlotResponseDto>.CreateFail("MasterTimeSlot not found", null, 404));
            return Ok(ApiResponse<MasterTimeSlotResponseDto>.CreateSuccess(updated));
        }

        [HttpPatch("{id}/is-active")]
        public async Task<IActionResult> UpdateIsActive(Guid id, [FromQuery] bool isActive)
        {
            var updated = await _service.UpdateIsActiveAsync(id, isActive);
            if (updated == null) return NotFound(ApiResponse<MasterTimeSlotResponseDto>.CreateFail("MasterTimeSlot not found", null, 404));
            return Ok(ApiResponse<MasterTimeSlotResponseDto>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound(ApiResponse<string>.CreateFail("MasterTimeSlot not found", null, 404));
            return Ok(ApiResponse<string>.CreateSuccess("Deleted"));
        }
    }
}

