using EVMManagement.BLL.DTOs.Request.Customer;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Customer;
using EVMManagement.BLL.Exceptions;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CustomersController : BaseController
    {
        public CustomersController(IServiceFacade services) : base(services)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await Services.CustomerService.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<CustomerResponse>>.CreateSuccess(result));
        }

        [HttpGet("dealer/{dealerId}")]
        public async Task<IActionResult> GetByDealerId(Guid dealerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Số trang và kích thước trang phải lớn hơn 0", null, 400));
            }

            var result = await Services.CustomerService.GetByDealerIdAsync(dealerId, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<CustomerResponse>>.CreateSuccess(result));
        }

        [HttpGet("managed-by")]
        [Authorize]
        public async Task<IActionResult> GetByManagedBy([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Số trang và kích thước trang phải lớn hơn 0", null, 400));
            }

            var managedById = GetCurrentAccountId();
            if (!managedById.HasValue)
            {
                return Unauthorized(ApiResponse<string>.CreateFail("Không xác định được tài khoản cần tra cứu", null, 401));
            }

            var result = await Services.CustomerService.GetByManagedByAsync(managedById.Value, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<CustomerResponse>>.CreateSuccess(result));
        }

        [HttpGet("managed-by/sales")]
        [Authorize]
        public async Task<IActionResult> GetSalesByManagedBy([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var managedById = GetCurrentAccountId();
            if (!managedById.HasValue)
            {
                return Unauthorized(ApiResponse<string>.CreateFail("Không xác định được tài khoản đăng nhập", null, 401));
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Thời gian bắt đầu phải nhỏ hơn hoặc bằng thời gian kết thúc", null, 400));
            }

            var summary = await Services.CustomerService.GetSalesSummaryByManagedAccountAsync(managedById.Value, fromDate, toDate);
            return Ok(ApiResponse<CustomerSalesSummaryResponse>.CreateSuccess(summary));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await Services.CustomerService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<CustomerResponse>.CreateFail("Customer not found", null, 404));
            return Ok(ApiResponse<CustomerResponse>.CreateSuccess(item));
        }

        [HttpGet("search/by-phone")]
        public async Task<IActionResult> SearchByPhone([FromQuery] string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return BadRequest(ApiResponse<CustomerResponse>.CreateFail("Phone number is required", null, 400));
            }

            var item = await Services.CustomerService.SearchCustomerByPhoneAsync(phone);
            if (item == null) return NotFound(ApiResponse<CustomerResponse>.CreateFail("Customer not found", null, 404));
            return Ok(ApiResponse<CustomerResponse>.CreateSuccess(item));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CustomerCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
                var errorMessage = errors.Count switch
                {
                    0 => "Dữ liệu không hợp lệ.",
                    1 => errors[0],
                    _ => string.Join("; ", errors)
                };

                return BadRequest(ApiResponse<Customer>.CreateFail(errorMessage, null, 400));
            }

            var accountId = GetCurrentAccountId();
            if (!accountId.HasValue)
            {
                return Unauthorized(ApiResponse<Customer>.CreateFail("Không xác định được tài khoản đăng nhập", null, 401));
            }

            try
            {
                var created = await Services.CustomerService.CreateCustomerAsync(dto, accountId.Value);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<Customer>.CreateSuccess(created));
            }
            catch (CustomerValidationException ex)
            {
                var errors = ex.Errors?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList() ?? new System.Collections.Generic.List<string>();
                var errorMessage = errors.Count switch
                {
                    0 => ex.Message,
                    1 => errors[0],
                    _ => string.Join("; ", errors)
                };

                return BadRequest(ApiResponse<Customer>.CreateFail(errorMessage, null, 400));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CustomerUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                var errorMessage = errors.Count switch
                {
                    0 => "Dữ liệu không hợp lệ.",
                    1 => errors[0],
                    _ => string.Join("; ", errors)
                };

                return BadRequest(ApiResponse<CustomerResponse>.CreateFail(errorMessage, null, 400));
            }

            try
            {
                var updated = await Services.CustomerService.UpdateAsync(id, dto);
                if (updated == null) return NotFound(ApiResponse<CustomerResponse>.CreateFail("Customer not found", null, 404));
                return Ok(ApiResponse<CustomerResponse>.CreateSuccess(updated));
            }
            catch (CustomerValidationException ex)
            {
                var errors = ex.Errors?.Where(e => !string.IsNullOrWhiteSpace(e)).ToList() ?? new System.Collections.Generic.List<string>();
                var errorMessage = errors.Count switch
                {
                    0 => ex.Message,
                    1 => errors[0],
                    _ => string.Join("; ", errors)
                };

                return BadRequest(ApiResponse<CustomerResponse>.CreateFail(errorMessage, null, 400));
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await Services.CustomerService.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<CustomerResponse>.CreateFail("Customer not found", null, 404));
            return Ok(ApiResponse<CustomerResponse>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await Services.CustomerService.DeleteAsync(id);
            if (!deleted) return NotFound(ApiResponse<string>.CreateFail("Customer not found", null, 404));
            return Ok(ApiResponse<string>.CreateSuccess("Deleted"));
        }
    }
}
