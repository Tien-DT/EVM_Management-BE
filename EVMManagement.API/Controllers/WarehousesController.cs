using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.Warehouse;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Warehouse;
using EVMManagement.DAL.Models.Entities;
using System.Threading.Tasks;
using System;
using System.Linq;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class WarehousesController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public WarehousesController(IServiceFacade services)
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

            var result = await _services.WarehouseService.GetAllWarehousesAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<WarehouseResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("dealer/{dealerId}")]
        public async Task<IActionResult> GetByDealerId(Guid dealerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.WarehouseService.GetWarehousesByDealerIdAsync(dealerId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<WarehouseResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.WarehouseService.GetWarehouseByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<WarehouseResponseDto>.CreateFail("Warehouse not found", null, 404));
            return Ok(ApiResponse<WarehouseResponseDto>.CreateSuccess(item));
        }

        [HttpPost]
        [Authorize(Roles = "DEALER_MANAGER,EVM_ADMIN")]
        public async Task<IActionResult> Create([FromBody] WarehouseCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<Warehouse>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _services.WarehouseService.CreateWarehouseAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<WarehouseResponseDto>.CreateSuccess(created));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "DEALER_MANAGER,EVM_ADMIN")]
        public async Task<IActionResult> Update(Guid id, [FromBody] WarehouseUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<WarehouseResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _services.WarehouseService.UpdateWarehouseAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<WarehouseResponseDto>.CreateFail("Warehouse not found", null, 404));
            return Ok(ApiResponse<WarehouseResponseDto>.CreateSuccess(updated));
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "DEALER_MANAGER,EVM_ADMIN")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await _services.WarehouseService.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<WarehouseResponseDto>.CreateFail("Warehouse not found", null, 404));
            return Ok(ApiResponse<WarehouseResponseDto>.CreateSuccess(updated));
        }

        
    }
}
