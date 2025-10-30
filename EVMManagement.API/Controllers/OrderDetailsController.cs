using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.OrderDetail;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.OrderDetail;
using EVMManagement.DAL.Models.Entities;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class OrderDetailsController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public OrderDetailsController(IServiceFacade services)
        {
            _services = services;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.OrderDetailService.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<OrderDetailResponse>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.OrderDetailService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<OrderDetailResponse>.CreateFail("OrderDetail not found", null, 404));
            return Ok(ApiResponse<OrderDetailResponse>.CreateSuccess(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderDetailCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<OrderDetail>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _services.OrderDetailService.CreateOrderDetailAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<OrderDetail>.CreateSuccess(created));
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] List<OrderDetailCreateDto> dtos)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<List<OrderDetail>>.CreateFail("Validation failed", errors, 400));
            }

            if (dtos == null || dtos.Count == 0)
            {
                return BadRequest(ApiResponse<List<OrderDetail>>.CreateFail("Order details list cannot be empty", null, 400));
            }

            var created = await _services.OrderDetailService.CreateOrderDetailsAsync(dtos);
            return Ok(ApiResponse<List<OrderDetail>>.CreateSuccess(created));
        }

        [HttpPost("/api/v2/orderdetails")]
        public async Task<IActionResult> CreateV2([FromBody] List<OrderDetailCreateDto> dtos)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<OrderDetailBulkCreateResponse>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            if (dtos == null || dtos.Count == 0)
            {
                return BadRequest(ApiResponse<OrderDetailBulkCreateResponse>.CreateFail("Danh sách chi tiết đơn hàng không được để trống", null, 400));
            }

            try
            {
                var result = await _services.OrderDetailService.CreateOrderDetailsV2Async(dtos);
                return Ok(ApiResponse<OrderDetailBulkCreateResponse>.CreateSuccess(result));
            }
            catch (ArgumentException ex)
            {
                var errors = new List<string> { ex.Message };
                return BadRequest(ApiResponse<OrderDetailBulkCreateResponse>.CreateFail(ex.Message, errors, 400));
            }
            catch (KeyNotFoundException ex)
            {
                var errors = new List<string> { ex.Message };
                return NotFound(ApiResponse<OrderDetailBulkCreateResponse>.CreateFail(ex.Message, errors, 404));
            }
            catch (InvalidOperationException ex)
            {
                var errors = new List<string> { ex.Message };
                return Conflict(ApiResponse<OrderDetailBulkCreateResponse>.CreateFail(ex.Message, errors, 409));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrderDetailUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<OrderDetailResponse>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _services.OrderDetailService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<OrderDetailResponse>.CreateFail("OrderDetail not found", null, 404));
            return Ok(ApiResponse<OrderDetailResponse>.CreateSuccess(updated));
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await _services.OrderDetailService.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<OrderDetailResponse>.CreateFail("OrderDetail not found", null, 404));
            return Ok(ApiResponse<OrderDetailResponse>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _services.OrderDetailService.DeleteAsync(id);
            if (!deleted) return NotFound(ApiResponse<string>.CreateFail("OrderDetail not found", null, 404));
            return Ok(ApiResponse<string>.CreateSuccess("Deleted"));
        }
    }
}
