using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.TestDriveBooking;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.TestDriveBooking;
using EVMManagement.API.Services;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class TestDriveBookingsController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public TestDriveBookingsController(IServiceFacade services)
        {
            _services = services;
        }

        [HttpPost]
        [Authorize(Roles = "DEALER_STAFF")]
        public async Task<IActionResult> Create([FromBody] TestDriveBookingCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<TestDriveBookingResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            try
            {
               
                var created = await _services.TestDriveBookingService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<TestDriveBookingResponseDto>.CreateSuccess(created));
            }
            catch (Exception ex)
            {
                var errors = new List<string> { ex.Message };
                return BadRequest(ApiResponse<TestDriveBookingResponseDto>.CreateFail(
                    "Failed to create test drive booking", errors, 400));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? vehicleTimeSlotId,
            [FromQuery] Guid? customerId,
            [FromQuery] Guid? dealerStaffId,
            [FromQuery] EVMManagement.DAL.Models.Enums.TestDriveBookingStatus? status,
            [FromQuery] Guid? dealerId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var filterDto = new TestDriveBookingFilterDto
            {
                VehicleTimeSlotId = vehicleTimeSlotId,
                CustomerId = customerId,
                DealerStaffId = dealerStaffId,
                Status = status,
                DealerId = dealerId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _services.TestDriveBookingService.GetByFilterAsync(filterDto);
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

        [HttpPost("send-reminder")]
        [Authorize(Roles = "DEALER_STAFF")]
        public async Task<IActionResult> SendReminder([FromQuery] List<Guid> ids)
        {
            // Validate input
            if (ids == null || ids.Count == 0)
            {
                return BadRequest(ApiResponse<string>.CreateFail(
                    "At least one booking ID is required", null, 400));
            }

            try
            {
                // Process based on count
                if (ids.Count == 1)
                {
                    // Single processing - throw exception if fails
                    await _services.TestDriveBookingService.SendReminderEmailAsync(ids[0]);
                    return Ok(ApiResponse<string>.CreateSuccess(
                        "Reminder email sent successfully", 
                        "Email sent at " + DateTime.UtcNow));
                }
                else
                {
                    // Bulk processing - return detailed results
                    var result = await _services.TestDriveBookingService.BulkSendReminderEmailAsync(ids);
                    
                    if (result.SuccessCount == 0)
                    {
                        return BadRequest(ApiResponse<BulkReminderResultDto>.CreateSuccess(
                            result, "All reminder emails failed to send"));
                    }
                    
                    if (result.FailureCount > 0)
                    {
                        return Ok(ApiResponse<BulkReminderResultDto>.CreateSuccess(
                            result, $"Partially successful: {result.SuccessCount}/{result.TotalRequested} emails sent"));
                    }

                    return Ok(ApiResponse<BulkReminderResultDto>.CreateSuccess(
                        result, $"All {result.SuccessCount} reminder emails sent successfully"));
                }
            }
            catch (Exception ex)
            {
                var errors = new List<string> { ex.Message };
                return BadRequest(ApiResponse<string>.CreateFail(
                    "Failed to send reminder email(s)", errors, 400));
            }
        }

       
    }
}
