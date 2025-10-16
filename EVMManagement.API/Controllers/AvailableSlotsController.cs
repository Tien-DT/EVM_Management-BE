using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.BLL.DTOs.Request.AvailableSlot;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.AvailableSlot;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AvailableSlotsController : ControllerBase
    {
        private readonly IAvailableSlotService _service;

        public AvailableSlotsController(IAvailableSlotService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AvailableSlotCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AvailableSlotResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _service.CreateAvailableSlotAsync(dto);
            return CreatedAtAction(nameof(Create), new { id = created.Id }, ApiResponse<AvailableSlotResponseDto>.CreateSuccess(created));
        }
    }
}

