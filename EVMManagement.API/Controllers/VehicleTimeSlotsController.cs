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

      
    }
}

