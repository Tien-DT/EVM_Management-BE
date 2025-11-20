using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.VehicleTimeSlot;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.VehicleTimeSlot;
using EVMManagement.DAL.Models.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using EVMManagement.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    //[Authorize]
    public class VehicleTimeSlotsController : BaseController
    {
        private readonly IServiceFacade _services;

        public VehicleTimeSlotsController(IServiceFacade services) : base(services)
        {
            _services = services;
        }

        /* Disabled - frontend only uses bulk-assign
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleTimeSlotCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<VehicleTimeSlotResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _services.VehicleTimeSlotService.CreateVehicleTimeSlotAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<VehicleTimeSlotResponseDto>.CreateSuccess(created));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.VehicleTimeSlotService.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleTimeSlotResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("available-vehicles-for-slot")]
        [Authorize(Roles = "DEALER_MANAGER")]
        public async Task<IActionResult> GetAvailableVehiclesForSlot(
            [FromQuery] Guid dealerId,
            [FromQuery] DateTime slotDate,
            [FromQuery] Guid masterSlotId)
        {
            if (dealerId == Guid.Empty || slotDate == DateTime.MinValue || masterSlotId == Guid.Empty)
            {
                return BadRequest(ApiResponse<AvailableVehiclesForSlotDto>.CreateFail(
                    "DealerId, SlotDate and MasterSlotId are required",
                    new List<string> { "All parameters are required" },
                    400));
            }

            var result = await _services.VehicleTimeSlotService.GetAvailableVehiclesForSlotAsync(
                dealerId, slotDate, masterSlotId);

            return Ok(ApiResponse<AvailableVehiclesForSlotDto>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.VehicleTimeSlotService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<VehicleTimeSlotResponseDto>.CreateFail("VehicleTimeSlot not found", null, 404));
            return Ok(ApiResponse<VehicleTimeSlotResponseDto>.CreateSuccess(item));
        }

        [HttpGet("by-vehicle/{vehicleId}")]
        public async Task<IActionResult> GetByVehicleId(Guid vehicleId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.VehicleTimeSlotService.GetByVehicleIdAsync(vehicleId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleTimeSlotResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("by-dealer/{dealerId}")]
        public async Task<IActionResult> GetByDealerId(Guid dealerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.VehicleTimeSlotService.GetByDealerIdAsync(dealerId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleTimeSlotResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("by-status")]
        public async Task<IActionResult> GetByStatus([FromQuery] TimeSlotStatus status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.VehicleTimeSlotService.GetByStatusAsync(status, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleTimeSlotResponseDto>>.CreateSuccess(result));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] VehicleTimeSlotUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<VehicleTimeSlotResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _services.VehicleTimeSlotService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<VehicleTimeSlotResponseDto>.CreateFail("VehicleTimeSlot not found", null, 404));
            return Ok(ApiResponse<VehicleTimeSlotResponseDto>.CreateSuccess(updated));
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] TimeSlotStatus status)
        {
            var updated = await _services.VehicleTimeSlotService.UpdateStatusAsync(id, status);
            if (updated == null) return NotFound(ApiResponse<VehicleTimeSlotResponseDto>.CreateFail("VehicleTimeSlot not found", null, 404));
            return Ok(ApiResponse<VehicleTimeSlotResponseDto>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await _services.VehicleTimeSlotService.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<VehicleTimeSlotResponseDto>.CreateFail("VehicleTimeSlot not found", null, 404));
            return Ok(ApiResponse<VehicleTimeSlotResponseDto>.CreateSuccess(updated));
        }

      
        [HttpGet("available-slots-in-date")]
        [Authorize(Roles = "DEALER_MANAGER,DEALER_STAFF")]
        public async Task<IActionResult> GetAvailableSlotsInDate(
            [FromQuery] Guid modelId,
            [FromQuery] Guid dealerId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
           
            if (modelId == Guid.Empty || dealerId == Guid.Empty)
            {
                return BadRequest(ApiResponse<List<DaySlotsummaryDto>>.CreateFail(
                    "ModelId and DealerId are required", null, 400));
            }
            var currentRole = GetCurrentRole();
            if (!currentRole.HasValue)
            {
                return Unauthorized(ApiResponse<string>.CreateFail("Không tìm thấy thông tin role của tài khoản.", errorCode: 401));
            }
            var result = await _services.VehicleTimeSlotService.GetAvailableSlotByModelAsync(
                modelId, dealerId, fromDate, toDate);
            return Ok(ApiResponse<List<DaySlotsummaryDto>>.CreateSuccess(result));
        }

        [HttpGet("available-by-model/vehicles-in-slot")]
        [Authorize(Roles = "DEALER_MANAGER,DEALER_STAFF")]
        public async Task<IActionResult> GetAvailableVehiclesInSlot(
            [FromQuery] DateTime slotDate,
            [FromQuery] Guid masterSlotId,
            [FromQuery] Guid modelId,
            [FromQuery] Guid dealerId)
        {
            var currentRole = GetCurrentRole();
            if (!currentRole.HasValue)
            {
                return Unauthorized(ApiResponse<string>.CreateFail("Không tìm thấy thông tin role của tài khoản.", errorCode: 401));
            }
            if (slotDate == DateTime.MinValue || masterSlotId == Guid.Empty || modelId == Guid.Empty || dealerId == Guid.Empty)
            {
                return BadRequest(ApiResponse<SlotVehiclesDto>.CreateFail(
                    "SlotDate, MasterSlotId, ModelId and DealerId are required", null, 400));
            }

            var result = await _services.VehicleTimeSlotService.GetAvailableVehiclesBySlotAsync(
                modelId, dealerId, slotDate, masterSlotId);
            
            if (result == null)
            {
                return NotFound(ApiResponse<SlotVehiclesDto>.CreateFail(
                    "No slot found for the given criteria", null, 404));
            }

            return Ok(ApiResponse<SlotVehiclesDto>.CreateSuccess(result));
        }

        */

        [HttpPost("bulk-assign")]
        [Authorize(Roles = "DEALER_MANAGER")]
        public async Task<IActionResult> BulkAssignVehicles([FromBody] BulkAssignVehiclesDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<BulkAssignResultDto>.CreateFail("Validation failed", errors, 400));
            }

            var dealerId = await GetCurrentUserDealerIdAsync();
            if (!dealerId.HasValue)
            {
                return Unauthorized(ApiResponse<BulkAssignResultDto>.CreateFail(
                    "Dealer manager must belong to a dealer", errorCode: 401));
            }

            var masterSlot = await _services.MasterTimeSlotService.GetByIdAsync(dto.MasterSlotId);
            if (masterSlot == null)
            {
                return NotFound(ApiResponse<BulkAssignResultDto>.CreateFail(
                    "Master time slot not found", errorCode: 404));
            }

            var result = await _services.VehicleTimeSlotService.BulkAssignVehiclesToSlotAsync(dto, dealerId.Value);

            if (result.FailureCount == result.TotalRequested)
            {
                return BadRequest(ApiResponse<BulkAssignResultDto>.CreateFail(
                    "All vehicle assignments failed", 
                    new List<string> { $"{result.FailureCount} out of {result.TotalRequested} vehicles could not be assigned" }, 
                    400));
            }

            if (result.FailureCount > 0)
            {
                return Ok(ApiResponse<BulkAssignResultDto>.CreateSuccess(
                    result, $"Partially successful: {result.SuccessCount} assigned, {result.FailureCount} failed"));
            }

            return Ok(ApiResponse<BulkAssignResultDto>.CreateSuccess(
                result, $"Successfully assigned {result.SuccessCount} vehicles to slot"));
        }

        /* Disabled - remaining vehicle time slot endpoints unused by frontend
        [HttpGet("slots-by-date")]
        [Authorize(Roles = "DEALER_MANAGER,DEALER_STAFF")]
        public async Task<IActionResult> GetSlotsByDate(
            [FromQuery] Guid dealerId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] TimeSlotStatus? status = null)
        {
            if (dealerId == Guid.Empty)
            {
                return BadRequest(ApiResponse<List<DateSlotGroupDto>>.CreateFail(
                    "DealerId is required",
                    new List<string> { "DealerId parameter is required" },
                    400));
            }

            // Default date range: today to +7 days
            var from = fromDate ?? DateTime.Today;
            var to = toDate ?? DateTime.Today.AddDays(7);

            // Call service without modelId parameter - get all vehicles for test drive schedule
            var result = await _services.VehicleTimeSlotService.GetSlotsByDateAsync(
                dealerId, from, to, status);

            if (result == null || result.Count == 0)
            {
                return NotFound(ApiResponse<List<DateSlotGroupDto>>.CreateFail(
                    "No slots found",
                    new List<string> { "No vehicle time slots found for the given date range" },
                    404));
            }

            return Ok(ApiResponse<List<DateSlotGroupDto>>.CreateSuccess(result));
        }
        */
    }
}

