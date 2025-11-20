using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EVMManagement.BLL.DTOs.Request.MasterTimeSlot;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.MasterTimeSlot;
using System;
using System.Threading.Tasks;
using System.Linq;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class MasterTimeSlotsController : BaseController
    {
        public MasterTimeSlotsController(IServiceFacade services) : base(services)
        {
        }

        [HttpPost]
        [Authorize(Roles = "DEALER_MANAGER")]
        public async Task<IActionResult> Create([FromBody] MasterTimeSlotCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<MasterTimeSlotResponseDto>.CreateFail("Validation failed. Time slot must be within business hours: 7:30 - 11:30 or 13:30 - 17:30", errors, 400));
            }

           
            if (IsDealerManager())
            {
                var dealerId = await GetCurrentUserDealerIdAsync();
                if (!dealerId.HasValue)
                {
                    return BadRequest(ApiResponse<MasterTimeSlotResponseDto>.CreateFail("Dealer ID not found for current user", null, 400));
                }
                dto.DealerId = dealerId.Value;
            }

            try
            {
                var created = await Services.MasterTimeSlotService.CreateMasterTimeSlotAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<MasterTimeSlotResponseDto>.CreateSuccess(created));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<MasterTimeSlotResponseDto>.CreateFail(ex.Message, null, 400));
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

           
            if (IsDealerManager())
            {
                var dealerId = await GetCurrentUserDealerIdAsync();
                if (!dealerId.HasValue)
                {
                    return BadRequest(ApiResponse<PagedResult<MasterTimeSlotResponseDto>>.CreateFail("Dealer ID not found for current user", null, 400));
                }
                var result = await Services.MasterTimeSlotService.GetByDealerIdAsync(dealerId.Value, pageNumber, pageSize);
                return Ok(ApiResponse<PagedResult<MasterTimeSlotResponseDto>>.CreateSuccess(result));
            }

            var allResult = await Services.MasterTimeSlotService.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<MasterTimeSlotResponseDto>>.CreateSuccess(allResult));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await Services.MasterTimeSlotService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<MasterTimeSlotResponseDto>.CreateFail("MasterTimeSlot not found", null, 404));
            
            if (IsDealerManager())
            {
                var dealerId = await GetCurrentUserDealerIdAsync();
                if (item.DealerId != dealerId)
                {
                    return Forbid();
                }
            }
            
            return Ok(ApiResponse<MasterTimeSlotResponseDto>.CreateSuccess(item));
        }

        /* Disabled - frontend not calling global active list
        [HttpGet("active")]
        [Authorize(Roles = "DEALER_MANAGER, DEALER_STAFF")]
        public async Task<IActionResult> GetActive([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            if (IsDealerManager())
            {
                var dealerId = await GetCurrentUserDealerIdAsync();
                if (!dealerId.HasValue)
                {
                    return BadRequest(ApiResponse<PagedResult<MasterTimeSlotResponseDto>>.CreateFail("Dealer ID not found for current user", null, 400));
                }
                var result = await Services.MasterTimeSlotService.GetActiveByDealerIdAsync(dealerId.Value, pageNumber, pageSize);
                return Ok(ApiResponse<PagedResult<MasterTimeSlotResponseDto>>.CreateSuccess(result));
            }

            var activeResult = await Services.MasterTimeSlotService.GetActiveAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<MasterTimeSlotResponseDto>>.CreateSuccess(activeResult));
        }

        */
        [HttpPut("{id}")]
        [Authorize(Roles = "DEALER_MANAGER")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MasterTimeSlotUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<MasterTimeSlotResponseDto>.CreateFail("Validation failed. Time slot must be within business hours: 7:30 - 11:30 or 13:30 - 17:30", errors, 400));
            }

            if (IsDealerManager())
            {
                var existing = await Services.MasterTimeSlotService.GetByIdAsync(id);
                if (existing == null) return NotFound(ApiResponse<MasterTimeSlotResponseDto>.CreateFail("MasterTimeSlot not found", null, 404));
                
                var dealerId = await GetCurrentUserDealerIdAsync();
                if (existing.DealerId != dealerId)
                {
                    return Forbid();
                }
                dto.DealerId = dealerId.Value; 
            }

            try
            {
                var updated = await Services.MasterTimeSlotService.UpdateAsync(id, dto);
                if (updated == null) return NotFound(ApiResponse<MasterTimeSlotResponseDto>.CreateFail("MasterTimeSlot not found", null, 404));
                return Ok(ApiResponse<MasterTimeSlotResponseDto>.CreateSuccess(updated));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<MasterTimeSlotResponseDto>.CreateFail(ex.Message, null, 400));
            }
        }

        [HttpPatch("{id}/is-active")]
        [Authorize(Roles = "DEALER_MANAGER")]
        public async Task<IActionResult> UpdateIsActive(Guid id, [FromQuery] bool isActive)
        {
      
            if (IsDealerManager())
            {
                var existing = await Services.MasterTimeSlotService.GetByIdAsync(id);
                if (existing == null) return NotFound(ApiResponse<MasterTimeSlotResponseDto>.CreateFail("MasterTimeSlot not found", null, 404));
                
                var dealerId = await GetCurrentUserDealerIdAsync();
                if (existing.DealerId != dealerId)
                {
                    return Forbid();
                }
            }

            try
            {
                var updated = await Services.MasterTimeSlotService.UpdateIsActiveAsync(id, isActive);
                if (updated == null) return NotFound(ApiResponse<MasterTimeSlotResponseDto>.CreateFail("MasterTimeSlot not found", null, 404));
                return Ok(ApiResponse<MasterTimeSlotResponseDto>.CreateSuccess(updated));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<MasterTimeSlotResponseDto>.CreateFail(ex.Message, null, 400));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "DEALER_MANAGER")]
        public async Task<IActionResult> Delete(Guid id)
        {
           
            if (IsDealerManager())
            {
                var existing = await Services.MasterTimeSlotService.GetByIdAsync(id);
                if (existing == null) return NotFound(ApiResponse<string>.CreateFail("MasterTimeSlot not found", null, 404));
                
                var dealerId = await GetCurrentUserDealerIdAsync();
                if (existing.DealerId != dealerId)
                {
                    return Forbid();
                }
            }

            var deleted = await Services.MasterTimeSlotService.DeleteAsync(id);
            if (!deleted) return NotFound(ApiResponse<string>.CreateFail("MasterTimeSlot not found", null, 404));
            return Ok(ApiResponse<string>.CreateSuccess("Deleted"));
        }

        [HttpGet("dealer/{dealerId}")]
        [Authorize(Roles = "DEALER_MANAGER, DEALER_STAFF")]
        public async Task<IActionResult> GetByDealerId(Guid dealerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await Services.MasterTimeSlotService.GetByDealerIdAsync(dealerId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<MasterTimeSlotResponseDto>>.CreateSuccess(result));
        }

       
        /* Disabled - frontend queries dealer slots via main endpoints
        [HttpGet("dealer/{dealerId}/active")]
        [Authorize(Roles = "DEALER_MANAGER, DEALER_STAFF")]
        public async Task<IActionResult> GetActiveByDealerId(Guid dealerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await Services.MasterTimeSlotService.GetActiveByDealerIdAsync(dealerId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<MasterTimeSlotResponseDto>>.CreateSuccess(result));
        }
        */
    }
}

