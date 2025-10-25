
using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Vehicle;
using EVMManagement.DAL.Models.Entities;
using System.Threading.Tasks;
using System;
using System.Linq;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class VehicleVariantsController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public VehicleVariantsController(IServiceFacade services)
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

            var result = await _services.VehicleVariantService.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleVariantResponse>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.VehicleVariantService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<VehicleVariantResponse>.CreateFail("VehicleVariant not found", null, 404));
            return Ok(ApiResponse<VehicleVariantResponse>.CreateSuccess(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleVariantCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<VehicleVariant>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _services.VehicleVariantService.CreateVehicleVariantAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<VehicleVariant>.CreateSuccess(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] VehicleVariantUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<VehicleVariantResponse>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _services.VehicleVariantService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<VehicleVariantResponse>.CreateFail("VehicleVariant not found", null, 404));
            return Ok(ApiResponse<VehicleVariantResponse>.CreateSuccess(updated));
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await _services.VehicleVariantService.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<VehicleVariantResponse>.CreateFail("VehicleVariant not found", null, 404));
            return Ok(ApiResponse<VehicleVariantResponse>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _services.VehicleVariantService.DeleteAsync(id);
            if (!deleted) return NotFound(ApiResponse<string>.CreateFail("VehicleVariant not found", null, 404));
            return Ok(ApiResponse<string>.CreateSuccess("Deleted"));
        }

        [HttpGet("by-model/{modelId}")]
        public async Task<IActionResult> GetByModelId(Guid modelId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.VehicleVariantService.GetByModelIdAsync(modelId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleVariantResponse>>.CreateSuccess(result));
        }

        [HttpGet("by-dealer/{dealerId}")]
        public async Task<IActionResult> GetByDealerId(Guid dealerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.VehicleVariantService.GetByDealerIdAsync(dealerId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleVariantResponse>>.CreateSuccess(result));
        }
    }
}
