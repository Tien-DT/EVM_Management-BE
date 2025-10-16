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
            return CreatedAtAction(nameof(Create), new { id = created.Id }, ApiResponse<MasterTimeSlotResponseDto>.CreateSuccess(created));
        }
    }
}

