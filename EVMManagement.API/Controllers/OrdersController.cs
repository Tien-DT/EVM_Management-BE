using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.Order;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Order;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.API.Services;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public OrdersController(IServiceFacade services)
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

            var result = await _services.OrderService.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<OrderResponse>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.OrderService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<OrderResponse>.CreateFail("Order not found", null, 404));
            return Ok(ApiResponse<OrderResponse>.CreateSuccess(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<Order>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _services.OrderService.CreateOrderAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<Order>.CreateSuccess(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrderUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<OrderResponse>.CreateFail("Validation failed", errors, 400));
            }

            var updated = await _services.OrderService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<OrderResponse>.CreateFail("Order not found", null, 404));
            return Ok(ApiResponse<OrderResponse>.CreateSuccess(updated));
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await _services.OrderService.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<OrderResponse>.CreateFail("Order not found", null, 404));
            return Ok(ApiResponse<OrderResponse>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _services.OrderService.DeleteAsync(id);
            if (!deleted) return NotFound(ApiResponse<string>.CreateFail("Order not found", null, 404));
            return Ok(ApiResponse<string>.CreateSuccess("Deleted"));
        }
    }
}
