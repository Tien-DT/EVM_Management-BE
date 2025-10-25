using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.BLL.DTOs.Request.HandoverRecord;
using EVMManagement.BLL.DTOs.Response.HandoverRecord;
using System.Threading.Tasks;
using System;
using System.Linq;
using EVMManagement.BLL.DTOs.Response;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class HandoverRecordsController : ControllerBase
    {
        private readonly IHandoverRecordService _service;

        public HandoverRecordsController(IHandoverRecordService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] HandoverRecordCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<HandoverRecordResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<HandoverRecordResponseDto>.CreateSuccess(created));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _service.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<HandoverRecordResponseDto>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<HandoverRecordResponseDto>.CreateFail("HandoverRecord not found", null, 404));
            return Ok(ApiResponse<HandoverRecordResponseDto>.CreateSuccess(item));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] HandoverRecordUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<HandoverRecordResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<HandoverRecordResponseDto>.CreateFail("HandoverRecord not found", null, 404));
            return Ok(ApiResponse<HandoverRecordResponseDto>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await _service.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<HandoverRecordResponseDto>.CreateFail("HandoverRecord not found", null, 404));
            return Ok(ApiResponse<HandoverRecordResponseDto>.CreateSuccess(updated));
        }

       
    }
}
