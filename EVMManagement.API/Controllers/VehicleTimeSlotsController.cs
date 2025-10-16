using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.BLL.DTOs.Request.VehicleTimeSlot;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.VehicleTimeSlot;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class VehicleTimeSlotsController : ControllerBase
    {
        private readonly IVehicleTimeSlotService _service;

        public VehicleTimeSlotsController(IVehicleTimeSlotService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleTimeSlotCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<VehicleTimeSlotResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _service.CreateVehicleTimeSlotAsync(dto);
            return CreatedAtAction(nameof(Create), new { id = created.Id }, ApiResponse<VehicleTimeSlotResponseDto>.CreateSuccess(created));
        }
    }
}

