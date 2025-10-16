using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.BLL.DTOs.Request.DealerContract;
using EVMManagement.BLL.DTOs.Response.DealerContract;
using EVMManagement.BLL.DTOs.Response;
using System;
using System.Linq;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    
    public class DealerContractsController : ControllerBase
    {
        private readonly IDealerContractService _service;

        public DealerContractsController(IDealerContractService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));

            var res = await _service.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<DealerContractResponseDto>>.CreateSuccess(res));
        }

    [HttpGet("dealer/{dealerId}")]
    public async Task<IActionResult> GetByDealerId(Guid dealerId)
        {
            var item = await _service.GetByDealerIdAsync(dealerId);
            if (item == null) return NotFound(ApiResponse<DealerContractResponseDto>.CreateFail("DealerContract not found for dealer", null, 404));
            return Ok(ApiResponse<DealerContractResponseDto>.CreateSuccess(item));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<DealerContractResponseDto>.CreateFail("DealerContract not found", null, 404));
            return Ok(ApiResponse<DealerContractResponseDto>.CreateSuccess(item));
        }

        [HttpPost]
        [Authorize(Roles = "EVM_ADMIN,DEALER_ADMIN")]
    public async Task<IActionResult> Create([FromBody] DealerContractCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<DealerContractResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<DealerContractResponseDto>.CreateSuccess(created));
        }
    }
}
