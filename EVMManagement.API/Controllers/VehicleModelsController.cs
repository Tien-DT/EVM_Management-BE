using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Vehicle;
using EVMManagement.DAL.Models.Enums;


namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class VehicleModelsController : ControllerBase
    {
        private readonly IVehicleModelService _service;

        public VehicleModelsController(IVehicleModelService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleModelCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<VehicleModelResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _service.CreateVehicleModelAsync(dto);
            return Ok(ApiResponse<VehicleModelResponseDto>.CreateSuccess(created));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _service.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleModelResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("by-ranking")]
        public async Task<IActionResult> GetByRanking([FromQuery] VehicleModelRanking ranking, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _service.GetByRankingAsync(ranking, pageNumber, pageSize);
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

            var updated = await _service.UpdateVehicleModelAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<VehicleModelResponseDto>.CreateFail("VehicleModel not found", null, 404));
            return Ok(ApiResponse<VehicleModelResponseDto>.CreateSuccess(updated));
        }

        [HttpPatch("{id}/is-deleted")]
        public async Task<IActionResult> UpdateIsDeleted([FromRoute] Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await _service.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<VehicleModelResponseDto>.CreateFail("VehicleModel not found", null, 404));
            return Ok(ApiResponse<VehicleModelResponseDto>.CreateSuccess(updated));
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? q)
        {
            var results = await _service.SearchByQueryAsync(q);
            return Ok(ApiResponse<IEnumerable<VehicleModelResponseDto>>.CreateSuccess(results));
           
        }
    }
}
