using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.BLL.DTOs.Request.Warehouse;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Warehouse;
using EVMManagement.DAL.Models.Entities;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseService _service;

        public WarehousesController(IWarehouseService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _service.GetAllWarehousesAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<WarehouseResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetWarehouseByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<WarehouseResponseDto>.CreateFail("Warehouse not found", null, 404));
            return Ok(ApiResponse<WarehouseResponseDto>.CreateSuccess(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WarehouseCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<Warehouse>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _service.CreateWarehouseAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<WarehouseResponseDto>.CreateSuccess(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] WarehouseUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<WarehouseResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _service.UpdateWarehouseAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<WarehouseResponseDto>.CreateFail("Warehouse not found", null, 404));
            return Ok(ApiResponse<WarehouseResponseDto>.CreateSuccess(updated));
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await _service.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<WarehouseResponseDto>.CreateFail("Warehouse not found", null, 404));
            return Ok(ApiResponse<WarehouseResponseDto>.CreateSuccess(updated));
        }

        
    }
}
