using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.DTOs.Response.Vehicle;
using EVMManagement.BLL.DTOs.Response;
using System;
using System.Linq;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public VehiclesController(IServiceFacade services)
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

        [HttpPatch("{id}/is-deleted")]
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
    }
}
