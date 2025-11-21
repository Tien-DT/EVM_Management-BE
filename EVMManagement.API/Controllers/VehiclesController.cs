using EVMManagement.API.Services;
using EVMManagement.BLL.DTOs.Request.Vehicle;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Vehicle;
using EVMManagement.DAL.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class VehiclesController :  BaseController
    {
        private readonly IServiceFacade _services;

        public VehiclesController(IServiceFacade services) : base(services)
        {
            _services = services;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Số trang và kích thước trang phải lớn hơn 0", null, 400));
            }

            var result = await _services.VehicleService.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleResponseDto>>.CreateSuccess(result));
        }

        /* Disabled - frontend not using vehicle details listing
        [HttpGet("details")]
        public async Task<IActionResult> GetAllWithDetails([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Số trang và kích thước trang phải lớn hơn 0", null, 400));
            }

            var result = await _services.VehicleService.GetAllWithDetailsAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleDetailResponseDto>>.CreateSuccess(result));
        }
        */

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.VehicleService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<VehicleResponseDto>.CreateFail("Không tìm thấy xe", null, 404));
            return Ok(ApiResponse<VehicleResponseDto>.CreateSuccess(item));
        }

        /* Disabled - frontend does not create vehicles directly
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<VehicleResponseDto>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            var created = await _services.VehicleService.CreateVehicleAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<VehicleResponseDto>.CreateSuccess(created));
        }
        */

        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(Guid id, [FromBody] VehicleUpdateDto dto)
        {
            if (dto == null)
            {
                return BadRequest(ApiResponse<VehicleResponseDto>.CreateFail("Dữ liệu không hợp lệ", null, 400));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<VehicleResponseDto>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            if (!HasVehicleUpdateValues(dto))
            {
                return BadRequest(ApiResponse<VehicleResponseDto>.CreateFail("Không có dữ liệu để cập nhật", null, 400));
            }

            var updated = await _services.VehicleService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<VehicleResponseDto>.CreateFail("Không tìm thấy xe", null, 404));
            return Ok(ApiResponse<VehicleResponseDto>.CreateSuccess(updated));
        }

        /* Disabled - frontend not using vehicle mutation/search endpoints
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] VehicleUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<VehicleResponseDto>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            var updated = await _services.VehicleService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<VehicleResponseDto>.CreateFail("Không tìm thấy xe", null, 404));
            return Ok(ApiResponse<VehicleResponseDto>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> UpdateIsDeleted([FromRoute] Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await _services.VehicleService.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<VehicleResponseDto>.CreateFail("Không tìm thấy xe", null, 404));
            return Ok(ApiResponse<VehicleResponseDto>.CreateSuccess(updated));
        }

      

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var results = await _services.VehicleService.SearchByQueryAsync(q, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleResponseDto>>.CreateSuccess(results));

        }



        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] EVMManagement.BLL.DTOs.Request.Vehicle.VehicleFilterDto filter)
        {
            if (filter.PageNumber < 1 || filter.PageSize < 1)
                return BadRequest(ApiResponse<string>.CreateFail("Số trang và kích thước trang phải lớn hơn 0", null, 400));

            var results = await _services.VehicleService.GetByFilterAsync(filter);
            return Ok(ApiResponse<PagedResult<VehicleResponseDto>>.CreateSuccess(results));
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromQuery] VehicleStatus status)
        {
            var updated = await _services.VehicleService.UpdateStatusAsync(id, status);
            if (updated == null) return NotFound(ApiResponse<VehicleResponseDto>.CreateFail("Không tìm thấy xe", null, 404));
            return Ok(ApiResponse<VehicleResponseDto>.CreateSuccess(updated));
        }

        [HttpGet("check-stock")]
        public async Task<IActionResult> CheckStock([FromQuery] Guid variantId, [FromQuery] Guid dealerId, [FromQuery] int quantity)
        {
            if (variantId == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.CreateFail("VariantId là bắt buộc", null, 400));
            }

            if (dealerId == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.CreateFail("DealerId là bắt buộc", null, 400));
            }

            if (quantity < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Số lượng phải lớn hơn 0", null, 400));
            }

            var result = await _services.VehicleService.CheckStockAvailabilityAsync(variantId, dealerId, quantity);
            return Ok(ApiResponse<StockCheckResponseDto>.CreateSuccess(result));
        }
        */

        [HttpGet("dealer/{dealerId}/variant/{variantId}")]
        [Authorize(Roles = "DEALER_MANAGER,DEALER_STAFF")]
        public async Task<IActionResult> GetVehiclesByDealerAndVariant(Guid dealerId, Guid variantId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var currentRole = GetCurrentRole();
            if (!currentRole.HasValue)
            {
                return Unauthorized(ApiResponse<string>.CreateFail("Không tìm thấy thông tin role của tài khoản.", errorCode: 401));
            }

            if (dealerId == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.CreateFail("DealerId là bắt buộc", null, 400));
            }

            if (variantId == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.CreateFail("VariantId là bắt buộc", null, 400));
            }

            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Số trang và kích thước trang phải lớn hơn 0", null, 400));
            }

            var result = await _services.VehicleService.GetVehiclesByDealerAndVariantAsync(dealerId, variantId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<VehicleResponseDto>>.CreateSuccess(result));
        }

        private static bool HasVehicleUpdateValues(VehicleUpdateDto dto)
        {
            return dto.VariantId.HasValue
                   || dto.WarehouseId.HasValue
                   || !string.IsNullOrWhiteSpace(dto.Vin)
                   || dto.Status.HasValue
                   || dto.Purpose.HasValue
                   || dto.ImageUrl != null;
        }
    }
}
