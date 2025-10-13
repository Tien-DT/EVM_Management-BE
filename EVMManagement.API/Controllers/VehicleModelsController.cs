using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.DTOs.Response;


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
                return BadRequest(ApiResponse<EVMManagement.BLL.DTOs.Response.Vehicle.VehicleModelResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _service.CreateVehicleModelAsync(dto);
            return Ok(ApiResponse<EVMManagement.BLL.DTOs.Response.Vehicle.VehicleModelResponseDto>.CreateSuccess(created));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var models = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<EVMManagement.BLL.DTOs.Response.Vehicle.VehicleModelResponseDto>>.CreateSuccess(models));
        }

        [HttpGet("by-ranking")]
        public async Task<IActionResult> GetByRanking([FromQuery] EVMManagement.DAL.Models.Enums.VehicleModelRanking ranking)
        {
            var models = await _service.GetByRankingAsync(ranking);
            return Ok(ApiResponse<IEnumerable<EVMManagement.BLL.DTOs.Response.Vehicle.VehicleModelResponseDto>>.CreateSuccess(models));
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] EVMManagement.BLL.DTOs.Request.Vehicle.VehicleModelUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<EVMManagement.BLL.DTOs.Response.Vehicle.VehicleModelResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _service.UpdateVehicleModelAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<EVMManagement.BLL.DTOs.Response.Vehicle.VehicleModelResponseDto>.CreateFail("VehicleModel not found", null, 404));
            return Ok(ApiResponse<EVMManagement.BLL.DTOs.Response.Vehicle.VehicleModelResponseDto>.CreateSuccess(updated));
        }
    }
}
