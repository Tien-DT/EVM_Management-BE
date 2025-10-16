using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.BLL.DTOs.Request.Contract;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Contract;
using EVMManagement.DAL.Models.Entities;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _service;

        public ContractsController(IContractService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _service.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<ContractResponse>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<ContractResponse>.CreateFail("Contract not found", null, 404));
            return Ok(ApiResponse<ContractResponse>.CreateSuccess(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ContractCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<Contract>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _service.CreateContractAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<Contract>.CreateSuccess(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ContractUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<ContractResponse>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<ContractResponse>.CreateFail("Contract not found", null, 404));
            return Ok(ApiResponse<ContractResponse>.CreateSuccess(updated));
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PartialUpdate(Guid id, [FromBody] ContractUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<ContractResponse>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<ContractResponse>.CreateFail("Contract not found", null, 404));
            return Ok(ApiResponse<ContractResponse>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound(ApiResponse<string>.CreateFail("Contract not found", null, 404));
            return Ok(ApiResponse<string>.CreateSuccess("Deleted"));
        }
    }
}
