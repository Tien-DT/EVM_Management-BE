using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using EVMManagement.BLL.DTOs.Request.Warehouse;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Warehouse;
using EVMManagement.BLL.DTOs.Response.Vehicle;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using System.Collections.Generic;
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

        [HttpGet("evm")]
        public async Task<IActionResult> GetEvmWarehouseVehicles(
            [FromQuery] Guid warehouseId,
            [FromQuery] VehiclePurpose? purpose = null,
            [FromQuery] VehicleStatus? status = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (warehouseId == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Thông tin WarehouseId là bắt buộc.", null, 400));
            }

            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await Services.WarehouseService.GetVehiclesInEvmWarehouseAsync(warehouseId, purpose, status, pageNumber, pageSize);

            if (!result.Success)
            {
                var statusCode = result.ErrorCode ?? StatusCodes.Status400BadRequest;

                if (statusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound(result);
                }

                if (statusCode == StatusCodes.Status500InternalServerError)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, result);
                }

                return StatusCode(statusCode, result);
            }

            return Ok(result);
        }

        /* Disabled - frontend not using dealer-warehouse inventory endpoint
        [HttpGet("dealer-warehouse")]
        [Authorize(Roles = "DEALER_MANAGER, DEALER_STAFF")]
        public async Task<IActionResult> GetDealerWarehouseVehicles(
            [FromQuery] Guid warehouseId,
            [FromQuery] VehiclePurpose? purpose = null,
            [FromQuery] VehicleStatus? status = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (warehouseId == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Thông tin WarehouseId là bắt buộc.", null, 400));
            }

            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await Services.WarehouseService.GetVehiclesInDealerWarehouseAsync(warehouseId, purpose, status, pageNumber, pageSize);

            if (!result.Success)
            {
                var statusCode = result.ErrorCode ?? StatusCodes.Status400BadRequest;

                if (statusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound(result);
                }

                if (statusCode == StatusCodes.Status500InternalServerError)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, result);
                }

                return StatusCode(statusCode, result);
            }

            return Ok(result);
        }
        */

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

        [HttpGet("{warehouseId}/vehicles-by-model")]
        public async Task<IActionResult> GetVehiclesByModelInWarehouse(
            Guid warehouseId,
            [FromQuery] Guid modelId,
            [FromQuery] VehiclePurpose? purpose = null,
            [FromQuery] VehicleStatus? status = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (warehouseId == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.CreateFail("WarehouseId là bắt buộc.", null, 400));
            }

            if (modelId == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.CreateFail("ModelId là bắt buộc.", null, 400));
            }

            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await Services.WarehouseService.GetVehiclesByModelInWarehouseAsync(warehouseId, modelId, purpose, status, pageNumber, pageSize);

            if (!result.Success)
            {
                var statusCode = result.ErrorCode ?? StatusCodes.Status400BadRequest;

                if (statusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound(result);
                }

                if (statusCode == StatusCodes.Status500InternalServerError)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, result);
                }

                return StatusCode(statusCode, result);
            }

            return Ok(result);
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

            var currentRole = GetCurrentRole();
            if (!currentRole.HasValue)
            {
                return Unauthorized(ApiResponse<string>.CreateFail("Không tìm thấy thông tin role của tài khoản.", errorCode: 401));
            }

            Guid? currentUserDealerId = null;
            if (currentRole.Value == AccountRole.DEALER_MANAGER)
            {
                currentUserDealerId = await GetCurrentUserDealerIdAsync();
                if (!currentUserDealerId.HasValue)
                {
                    return BadRequest(ApiResponse<string>.CreateFail("Không tìm thấy thông tin dealer của bạn. Vui lòng liên hệ admin.", errorCode: 400));
                }
            }

            var result = await Services.WarehouseService.UpdateWarehouseAsync(id, dto, currentRole.Value, currentUserDealerId);
            
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

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "DEALER_MANAGER,EVM_ADMIN")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var currentRole = GetCurrentRole();
            if (!currentRole.HasValue)
            {
                return Unauthorized(ApiResponse<string>.CreateFail("Không tìm thấy thông tin role của tài khoản.", errorCode: 401));
            }

            Guid? currentUserDealerId = null;
            if (currentRole.Value == AccountRole.DEALER_MANAGER)
            {
                currentUserDealerId = await GetCurrentUserDealerIdAsync();
                if (!currentUserDealerId.HasValue)
                {
                    return BadRequest(ApiResponse<string>.CreateFail("Không tìm thấy thông tin dealer của bạn. Vui lòng liên hệ admin.", errorCode: 400));
                }
            }

            var result = await Services.WarehouseService.UpdateIsDeletedAsync(id, isDeleted, currentRole.Value, currentUserDealerId);
            
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

            return Ok(result);
        }

        /* Disabled - frontend does not use generic add-vehicles endpoint
        [HttpPost("add-vehicles")]
        [Authorize(Roles = "EVM_STAFF,EVM_ADMIN,DEALER_MANAGER")]
        public async Task<IActionResult> AddVehiclesToWarehouse([FromBody] AddVehiclesToWarehouseRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<string>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            var currentAccountId = GetCurrentAccountId();
            if (!currentAccountId.HasValue)
            {
                return Unauthorized(ApiResponse<string>.CreateFail("Không tìm thấy ID người dùng", null, 401));
            }

            var result = await Services.WarehouseService.AddVehiclesToWarehouseAsync(dto, currentAccountId.Value);

            if (!result.Success)
            {
                var statusCode = result.ErrorCode ?? StatusCodes.Status400BadRequest;

                if (statusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound(result);
                }

                if (statusCode == StatusCodes.Status500InternalServerError)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, result);
                }

                return StatusCode(statusCode, result);
            }

            return Ok(result);
        }
        */

        [HttpPost("evm/add-vehicles")]
        [Authorize(Roles = "EVM_STAFF,EVM_ADMIN")]
        public async Task<IActionResult> AddVehiclesToEvmWarehouse([FromBody] AddVehiclesToEvmWarehouseDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<string>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            var currentAccountId = GetCurrentAccountId();
            if (!currentAccountId.HasValue)
            {
                return Unauthorized(ApiResponse<string>.CreateFail("Không tìm thấy ID người dùng", null, 401));
            }

            var result = await Services.WarehouseService.AddVehiclesToEvmWarehouseAsync(dto, currentAccountId.Value);

            if (!result.Success)
            {
                var statusCode = result.ErrorCode ?? StatusCodes.Status400BadRequest;

                if (statusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound(result);
                }

                if (statusCode == StatusCodes.Status500InternalServerError)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, result);
                }

                return StatusCode(statusCode, result);
            }

            return Ok(result);
        }

        /* Disabled - frontend not using dealer add-vehicles endpoint
        [HttpPost("dealer/add-vehicles")]
        [Authorize(Roles = "EVM_STAFF,EVM_ADMIN,DEALER_MANAGER")]
        public async Task<IActionResult> AddVehiclesToDealerWarehouse([FromBody] AddVehiclesToDealerWarehouseDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<string>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            var currentAccountId = GetCurrentAccountId();
            if (!currentAccountId.HasValue)
            {
                return Unauthorized(ApiResponse<string>.CreateFail("Không tìm thấy ID người dùng", null, 401));
            }

            var result = await Services.WarehouseService.AddVehiclesToDealerWarehouseAsync(dto, currentAccountId.Value);

            if (!result.Success)
            {
                var statusCode = result.ErrorCode ?? StatusCodes.Status400BadRequest;

                if (statusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound(result);
                }

                if (statusCode == StatusCodes.Status500InternalServerError)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, result);
                }

                return StatusCode(statusCode, result);
            }

            return Ok(result);
        }
        */
    }
}
