using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EVMManagement.API.Services;
using EVMManagement.BLL.DTOs.Request.Transport;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Transport;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class TransportsController : BaseController
    {
        private readonly IServiceFacade _services;

        public TransportsController(IServiceFacade services) : base(services)
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

            var result = await _services.TransportService.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<TransportResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("dealer/{dealerId}")]
        public async Task<IActionResult> GetByDealer(Guid dealerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.TransportService.GetByDealerAsync(dealerId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<TransportResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _services.TransportService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse<TransportResponseDto>.CreateFail("Transport not found", null, 404));
            }

            return Ok(ApiResponse<TransportResponseDto>.CreateSuccess(result));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TransportCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<TransportResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            try
            {
                var created = await _services.TransportService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<TransportResponseDto>.CreateSuccess(created));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TransportResponseDto>.CreateFail(ex.Message, null, 400));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TransportUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<TransportResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _services.TransportService.UpdateAsync(id, dto);
            if (updated == null)
            {
                return NotFound(ApiResponse<TransportResponseDto>.CreateFail("Transport not found", null, 404));
            }

            return Ok(ApiResponse<TransportResponseDto>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _services.TransportService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<string>.CreateFail("Transport not found", null, 404));
            }

            return Ok(ApiResponse<string>.CreateSuccess("Transport deleted successfully"));
        }
    }
}

