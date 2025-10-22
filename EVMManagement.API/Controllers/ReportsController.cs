using System;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.API.Services;
using EVMManagement.BLL.DTOs.Request.Report;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Report;
using Microsoft.AspNetCore.Mvc;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public ReportsController(IServiceFacade services)
        {
            _services = services;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? dealerId, [FromQuery] Guid? accountId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.ReportService.GetAsync(dealerId, accountId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<ReportResponse>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.ReportService.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<ReportResponse>.CreateFail("Report not found", null, 404));
            }

            return Ok(ApiResponse<ReportResponse>.CreateSuccess(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReportCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<ReportResponse>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _services.ReportService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<ReportResponse>.CreateSuccess(created));
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ReportUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<ReportResponse>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _services.ReportService.UpdateAsync(id, dto);
            if (updated == null)
            {
                return NotFound(ApiResponse<ReportResponse>.CreateFail("Report not found", null, 404));
            }

            return Ok(ApiResponse<ReportResponse>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _services.ReportService.SoftDeleteAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<string>.CreateFail("Report not found", null, 404));
            }

            return Ok(ApiResponse<string>.CreateSuccess("Deleted"));
        }
    }
}
