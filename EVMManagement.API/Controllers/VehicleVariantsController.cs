
using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.DAL.Models.Entities;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class VehicleVariantsController : ControllerBase
    {
        private readonly IVehicleVariantService _service;

        public VehicleVariantsController(IVehicleVariantService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleVariantCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<VehicleVariant>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _service.CreateVehicleVariantAsync(dto);
            // Assuming GetById is not yet implemented, returning the created object directly.
            // For a complete implementation, a GetById action should be created.
            return Ok(ApiResponse<VehicleVariant>.CreateSuccess(created));
        }
    }
}
