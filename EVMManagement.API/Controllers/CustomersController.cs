using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.Customer;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Customer;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.API.Services;
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
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<Customer>.CreateFail("Validation failed", errors, 400));
            }

            var accountId = GetCurrentAccountId();
            if (!accountId.HasValue)
            {
                return Unauthorized(ApiResponse<Customer>.CreateFail("Không xác định được tài khoản đăng nhập", null, 401));
            }

            var created = await Services.CustomerService.CreateCustomerAsync(dto, accountId.Value);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<Customer>.CreateSuccess(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CustomerUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<CustomerResponse>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await Services.CustomerService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<CustomerResponse>.CreateFail("Customer not found", null, 404));
            return Ok(ApiResponse<CustomerResponse>.CreateSuccess(updated));
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
