using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using EVMManagement.BLL.DTOs.Request.Warehouse;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Warehouse;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using System.Threading.Tasks;
using System;
using System.Linq;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize]
    public class WarehousesController : BaseController
    {
        public WarehousesController(IServiceFacade services) : base(services)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await Services.WarehouseService.GetAllWarehousesAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<WarehouseResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("dealer/{dealerId}")]
        public async Task<IActionResult> GetByDealerId(Guid dealerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await Services.WarehouseService.GetWarehousesByDealerIdAsync(dealerId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<WarehouseResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await Services.WarehouseService.GetWarehouseByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<WarehouseResponseDto>.CreateFail("Warehouse not found", null, 404));
            return Ok(ApiResponse<WarehouseResponseDto>.CreateSuccess(item));
        }

        [HttpPost]
        [Authorize(Roles = "DEALER_MANAGER,EVM_ADMIN")]
        public async Task<IActionResult> Create([FromBody] WarehouseCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<Warehouse>.CreateFail("Validation failed", errors, 400));
            }

            // role  user hiện tại
            var currentRole = GetCurrentRole();
            if (!currentRole.HasValue)
            {
                return Unauthorized(ApiResponse<string>.CreateFail("Không tìm thấy thông tin role của tài khoản.", errorCode: 401));
            }

            // DealerId của user hiện tại nếu là DEALER_MANAGER
            Guid? currentUserDealerId = null;
            if (currentRole.Value == AccountRole.DEALER_MANAGER)
            {
                currentUserDealerId = await GetCurrentUserDealerIdAsync();
                if (!currentUserDealerId.HasValue)
                {
                    return BadRequest(ApiResponse<string>.CreateFail("Không tìm thấy thông tin dealer của bạn. Vui lòng liên hệ admin.", errorCode: 400));
                }
            }

            var result = await Services.WarehouseService.CreateWarehouseAsync(dto, currentRole.Value, currentUserDealerId);
            
            if (!result.Success)
            {
                var statusCode = result.ErrorCode ?? StatusCodes.Status400BadRequest;
                
                if (statusCode == StatusCodes.Status401Unauthorized)
                {
                    return Unauthorized(result);
                }

                if (statusCode == StatusCodes.Status403Forbidden)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, result);
                }

                if (statusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound(result);
                }

                return StatusCode(statusCode, result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "DEALER_MANAGER,EVM_ADMIN")]
        public async Task<IActionResult> Update(Guid id, [FromBody] WarehouseUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<WarehouseResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await Services.WarehouseService.UpdateWarehouseAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<WarehouseResponseDto>.CreateFail("Warehouse not found", null, 404));
            return Ok(ApiResponse<WarehouseResponseDto>.CreateSuccess(updated));
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "DEALER_MANAGER,EVM_ADMIN")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await Services.WarehouseService.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<WarehouseResponseDto>.CreateFail("Warehouse not found", null, 404));
            return Ok(ApiResponse<WarehouseResponseDto>.CreateSuccess(updated));
        }

        
    }
}
