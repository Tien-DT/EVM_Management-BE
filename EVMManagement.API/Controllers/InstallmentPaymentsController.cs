using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.API.Services;
using EVMManagement.BLL.DTOs.Request.InstallmentPayment;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.InstallmentPayment;
using EVMManagement.DAL.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class InstallmentPaymentsController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public InstallmentPaymentsController(IServiceFacade services)
        {
            _services = services;
        }

        /* Disabled - frontend does not use installment payment endpoints
        [HttpGet]
        public async Task<IActionResult> GetPayments(
            [FromQuery] Guid? id,
            [FromQuery] Guid? planId,
            [FromQuery] Guid? orderId,
            [FromQuery] Guid? customerId,
            [FromQuery] InstallmentPaymentStatus? status,
            [FromQuery] DateTime? dueDateFrom,
            [FromQuery] DateTime? dueDateTo,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var filter = new InstallmentPaymentFilterDto
            {
                Id = id,
                PlanId = planId,
                OrderId = orderId,
                CustomerId = customerId,
                Status = status,
                DueDateFrom = dueDateFrom,
                DueDateTo = dueDateTo,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            try
            {
                var result = await _services.InstallmentPaymentService.GetByFilterAsync(filter);
                return Ok(ApiResponse<PagedResult<InstallmentPaymentResponseDto>>.CreateSuccess(result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.CreateFail(ex.Message, null, 400));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var payment = await _services.InstallmentPaymentService.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound(ApiResponse<string>.CreateFail("Không tìm thấy kỳ thanh toán.", null, 404));
            }

            return Ok(ApiResponse<InstallmentPaymentResponseDto>.CreateSuccess(payment));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InstallmentPaymentCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<string>.CreateFail("Dữ liệu không hợp lệ.", errors, 400));
            }

            try
            {
                var created = await _services.InstallmentPaymentService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<InstallmentPaymentResponseDto>.CreateSuccess(created));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.CreateFail(ex.Message, null, 404));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.CreateFail(ex.Message, null, 409));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] InstallmentPaymentUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<string>.CreateFail("Dữ liệu không hợp lệ.", errors, 400));
            }

            try
            {
                var updated = await _services.InstallmentPaymentService.UpdateAsync(id, dto);
                if (updated == null)
                {
                    return NotFound(ApiResponse<string>.CreateFail("Không tìm thấy kỳ thanh toán.", null, 404));
                }

                return Ok(ApiResponse<InstallmentPaymentResponseDto>.CreateSuccess(updated));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.CreateFail(ex.Message, null, 404));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.CreateFail(ex.Message, null, 409));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _services.InstallmentPaymentService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<string>.CreateFail("Không tìm thấy kỳ thanh toán.", null, 404));
            }

            return Ok(ApiResponse<string>.CreateSuccess("Đã xóa kỳ thanh toán."));
        }
        */
    }
}
