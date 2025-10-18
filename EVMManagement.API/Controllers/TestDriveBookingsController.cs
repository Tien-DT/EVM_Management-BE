using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.TestDriveBooking;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.TestDriveBooking;
using EVMManagement.API.Services;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class TestDriveBookingsController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public TestDriveBookingsController(IServiceFacade services)
        {
            _services = services;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TestDriveBookingCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<TestDriveBookingResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _services.TestDriveBookingService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<TestDriveBookingResponseDto>.CreateSuccess(created));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.TestDriveBookingService.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<TestDriveBookingResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.TestDriveBookingService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<TestDriveBookingResponseDto>.CreateFail("TestDriveBooking not found", null, 404));
            return Ok(ApiResponse<TestDriveBookingResponseDto>.CreateSuccess(item));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TestDriveBookingUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<TestDriveBookingResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _services.TestDriveBookingService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<TestDriveBookingResponseDto>.CreateFail("TestDriveBooking not found", null, 404));
            return Ok(ApiResponse<TestDriveBookingResponseDto>.CreateSuccess(updated));
        }

        [HttpPatch("{id}/is-deleted")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await _services.TestDriveBookingService.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<TestDriveBookingResponseDto>.CreateFail("TestDriveBooking not found", null, 404));
            return Ok(ApiResponse<TestDriveBookingResponseDto>.CreateSuccess(updated));
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetByFilter([FromQuery] Guid? dealerId, [FromQuery] Guid? customerId, [FromQuery] EVMManagement.DAL.Models.Enums.TestDriveBookingStatus? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var filterDto = new TestDriveBookingFilterDto
            {
                DealerId = dealerId,
                CustomerId = customerId,
                Status = status,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _services.TestDriveBookingService.GetByFilterAsync(filterDto);
            return Ok(ApiResponse<PagedResult<TestDriveBookingResponseDto>>.CreateSuccess(result));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var item = await _services.TestDriveBookingService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<TestDriveBookingResponseDto>.CreateFail("TestDriveBooking not found", null, 404));

            var deleted = await _services.TestDriveBookingService.DeleteAsync(id);
            if (!deleted) return NotFound(ApiResponse<TestDriveBookingResponseDto>.CreateFail("TestDriveBooking not found", null, 404));

            return Ok(ApiResponse<TestDriveBookingResponseDto>.CreateSuccess(item));
        }
    }
}
