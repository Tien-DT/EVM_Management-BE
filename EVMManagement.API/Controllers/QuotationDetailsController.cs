using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.QuotationDetail;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.QuotationDetail;
using EVMManagement.DAL.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class QuotationDetailsController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public QuotationDetailsController(IServiceFacade services)
        {
            _services = services;
        }

        [HttpGet("quotation/{quotationId}")]
        public async Task<IActionResult> GetByQuotationId(Guid quotationId)
        {
            var items = await _services.QuotationDetailService.GetByQuotationIdAsync(quotationId);
            return Ok(ApiResponse<IList<QuotationDetailWithOrderResponse>>.CreateSuccess(items));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.QuotationDetailService.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<QuotationDetailResponse>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.QuotationDetailService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<QuotationDetailResponse>.CreateFail("QuotationDetail not found", null, 404));
            return Ok(ApiResponse<QuotationDetailResponse>.CreateSuccess(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuotationDetailCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<QuotationDetail>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _services.QuotationDetailService.CreateQuotationDetailAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<QuotationDetail>.CreateSuccess(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] QuotationDetailUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<QuotationDetailResponse>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _services.QuotationDetailService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<QuotationDetailResponse>.CreateFail("QuotationDetail not found", null, 404));
            return Ok(ApiResponse<QuotationDetailResponse>.CreateSuccess(updated));
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PartialUpdate(Guid id, [FromBody] QuotationDetailUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<QuotationDetailResponse>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _services.QuotationDetailService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<QuotationDetailResponse>.CreateFail("QuotationDetail not found", null, 404));
            return Ok(ApiResponse<QuotationDetailResponse>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _services.QuotationDetailService.DeleteAsync(id);
            if (!deleted) return NotFound(ApiResponse<string>.CreateFail("QuotationDetail not found", null, 404));
            return Ok(ApiResponse<string>.CreateSuccess("Deleted"));
        }
    }
}
