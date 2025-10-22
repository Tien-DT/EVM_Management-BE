using System;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.API.Services;
using EVMManagement.BLL.DTOs.Request.Deposit;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Deposit;
using Microsoft.AspNetCore.Mvc;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DepositsController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public DepositsController(IServiceFacade services)
        {
            _services = services;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? orderId, [FromQuery] Guid? receivedByUserId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.DepositService.GetAsync(orderId, receivedByUserId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<DepositResponse>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.DepositService.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<DepositResponse>.CreateFail("Deposit not found", null, 404));
            }

            return Ok(ApiResponse<DepositResponse>.CreateSuccess(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DepositCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<DepositResponse>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _services.DepositService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<DepositResponse>.CreateSuccess(created));
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DepositUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<DepositResponse>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _services.DepositService.UpdateAsync(id, dto);
            if (updated == null)
            {
                return NotFound(ApiResponse<DepositResponse>.CreateFail("Deposit not found", null, 404));
            }

            return Ok(ApiResponse<DepositResponse>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _services.DepositService.SoftDeleteAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<string>.CreateFail("Deposit not found", null, 404));
            }

            return Ok(ApiResponse<string>.CreateSuccess("Deleted"));
        }
    }
}
