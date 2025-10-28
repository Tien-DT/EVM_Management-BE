using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.Quotation;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Quotation;
using EVMManagement.DAL.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class QuotationsController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public QuotationsController(IServiceFacade services)
        {
            _services = services;
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(Guid customerId)
        {
            var items = await _services.QuotationService.GetByCustomerIdAsync(customerId);
            return Ok(ApiResponse<IList<QuotationResponseDto>>.CreateSuccess(items));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] QuotationStatus? status = null)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Số trang và kích thước trang phải lớn hơn 0", null, 400));
            }

            var result = await _services.QuotationService.GetAllAsync(pageNumber, pageSize, search, status);
            return Ok(ApiResponse<PagedResult<QuotationResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.QuotationService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<QuotationResponseDto>.CreateFail("Không tìm thấy báo giá", null, 404));
            return Ok(ApiResponse<QuotationResponseDto>.CreateSuccess(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQuotationDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<QuotationResponseDto>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            var created = await _services.QuotationService.CreateQuotationAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<QuotationResponseDto>.CreateSuccess(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQuotationDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<QuotationResponseDto>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            var updated = await _services.QuotationService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<QuotationResponseDto>.CreateFail("Không tìm thấy báo giá", null, 404));
            return Ok(ApiResponse<QuotationResponseDto>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var updated = await _services.QuotationService.UpdateIsDeletedAsync(id, true);
            if (updated == null) return NotFound(ApiResponse<QuotationResponseDto>.CreateFail("Không tìm thấy báo giá", null, 404));
            return Ok(ApiResponse<QuotationResponseDto>.CreateSuccess(updated));
        }
    }
}
