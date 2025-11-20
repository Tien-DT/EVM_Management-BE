using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.Invoice;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Invoice;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.API.Services;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public InvoicesController(IServiceFacade services)
        {
            _services = services;
        }

        /* Disabled - frontend does not use invoice endpoints
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.InvoiceService.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<InvoiceResponse>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.InvoiceService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<InvoiceResponse>.CreateFail("Invoice not found", null, 404));
            return Ok(ApiResponse<InvoiceResponse>.CreateSuccess(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InvoiceCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<Invoice>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _services.InvoiceService.CreateInvoiceAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<Invoice>.CreateSuccess(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] InvoiceUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<InvoiceResponse>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _services.InvoiceService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<InvoiceResponse>.CreateFail("Invoice not found", null, 404));
            return Ok(ApiResponse<InvoiceResponse>.CreateSuccess(updated));
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await _services.InvoiceService.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<InvoiceResponse>.CreateFail("Invoice not found", null, 404));
            return Ok(ApiResponse<InvoiceResponse>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _services.InvoiceService.DeleteAsync(id);
            if (!deleted) return NotFound(ApiResponse<string>.CreateFail("Invoice not found", null, 404));
            return Ok(ApiResponse<string>.CreateSuccess("Deleted"));
        }
        */
    }
}
