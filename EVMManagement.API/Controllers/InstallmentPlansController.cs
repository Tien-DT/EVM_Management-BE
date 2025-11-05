using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.API.Services;
using EVMManagement.BLL.DTOs.Request.InstallmentPlan;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.InstallmentPlan;
using EVMManagement.DAL.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class InstallmentPlansController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public InstallmentPlansController(IServiceFacade services)
        {
            _services = services;
        }

        [HttpGet]
        public async Task<IActionResult> GetPlans(
            [FromQuery] Guid? id,
            [FromQuery] Guid? orderId,
            [FromQuery] Guid? customerId,
            [FromQuery] InstallmentPlanStatus? status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var filter = new InstallmentPlanFilterDto
            {
                Id = id,
                OrderId = orderId,
                CustomerId = customerId,
                Status = status,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            try
            {
                var result = await _services.InstallmentPlanService.GetByFilterAsync(filter);
                return Ok(ApiResponse<PagedResult<InstallmentPlanResponseDto>>.CreateSuccess(result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.CreateFail(ex.Message, null, 400));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var plan = await _services.InstallmentPlanService.GetByIdAsync(id);
            if (plan == null)
            {
                return NotFound(ApiResponse<string>.CreateFail("Không tìm thấy kế hoạch trả góp.", null, 404));
            }

            return Ok(ApiResponse<InstallmentPlanResponseDto>.CreateSuccess(plan));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InstallmentPlanCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<string>.CreateFail("Dữ liệu không hợp lệ.", errors, 400));
            }

            try
            {
                var created = await _services.InstallmentPlanService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<InstallmentPlanResponseDto>.CreateSuccess(created));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.CreateFail(ex.Message, null, 404));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.CreateFail(ex.Message, null, 409));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.CreateFail("Đã xảy ra lỗi khi tạo kế hoạch trả góp.", new List<string> { ex.Message }, 500));
            }
        }
    }
}
