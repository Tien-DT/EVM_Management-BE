using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.Contract;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Contract;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.API.Services;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ContractsController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public ContractsController(IServiceFacade services)
        {
            _services = services;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? orderId, [FromQuery] Guid? customerId, [FromQuery] Guid? dealerId, [FromQuery] Guid? createdByUserId, [FromQuery] Guid? signedByUserId, [FromQuery] ContractStatus? status, [FromQuery] ContractType? contractType, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Số trang và kích thước trang phải lớn hơn 0", null, 400));
            }

            var result = await _services.ContractService.GetAllAsync(orderId, customerId, dealerId, createdByUserId, signedByUserId, status, contractType, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<ContractDetailResponse>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.ContractService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<ContractResponse>.CreateFail("Không tìm thấy hợp đồng", null, 404));
            return Ok(ApiResponse<ContractResponse>.CreateSuccess(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ContractCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<Contract>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            try
            {
                var created = await _services.ContractService.CreateContractAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<Contract>.CreateSuccess(created));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<Contract>.CreateFail(ex.Message, null, 400));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ContractUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<ContractResponse>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            var updated = await _services.ContractService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<ContractResponse>.CreateFail("Không tìm thấy hợp đồng", null, 404));
            return Ok(ApiResponse<ContractResponse>.CreateSuccess(updated));
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PartialUpdate(Guid id, [FromBody] ContractUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<ContractResponse>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            var updated = await _services.ContractService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<ContractResponse>.CreateFail("Không tìm thấy hợp đồng", null, 404));
            return Ok(ApiResponse<ContractResponse>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _services.ContractService.DeleteAsync(id);
            if (!deleted) return NotFound(ApiResponse<string>.CreateFail("Không tìm thấy hợp đồng", null, 404));
            return Ok(ApiResponse<string>.CreateSuccess("Đã xóa"));
        }

        [HttpGet("by-dealer/{dealerId}")]
        public async Task<IActionResult> GetByDealerId(Guid dealerId, [FromQuery] ContractStatus? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Số trang và kích thước trang phải lớn hơn 0", null, 400));
            }

            var result = await _services.ContractService.GetByDealerIdAsync(dealerId, status, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<ContractDetailResponse>>.CreateSuccess(result));
        }
    }
}




