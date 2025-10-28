using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.Order;
using EVMManagement.BLL.DTOs.Request.Deposit;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Order;
using EVMManagement.BLL.DTOs.Response.Deposit;
using EVMManagement.BLL.DTOs.Response.HandoverRecord;
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
                return BadRequest(ApiResponse<string>.CreateFail("Số trang và kích thước trang phải lớn hơn 0", null, 400));
            }

            var result = await _services.OrderService.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<OrderResponse>>.CreateSuccess(result));
        }

        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] OrderFilterDto filter)
        {
            if (filter.PageNumber < 1 || filter.PageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Số trang và kích thước trang phải lớn hơn 0", null, 400));
            }

            var result = await _services.OrderService.GetByFilterAsync(filter);
            return Ok(ApiResponse<PagedResult<OrderResponse>>.CreateSuccess(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.OrderService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<OrderResponse>.CreateFail("Không tìm thấy đơn hàng", null, 404));
            return Ok(ApiResponse<OrderResponse>.CreateSuccess(item));
        }

        [HttpGet("{id}/with-details")]
        public async Task<IActionResult> GetByIdWithDetails(Guid id)
        {
            var item = await _services.OrderService.GetByIdWithDetailsAsync(id);
            if (item == null) return NotFound(ApiResponse<OrderWithDetailsResponse>.CreateFail("Không tìm thấy đơn hàng", null, 404));
            return Ok(ApiResponse<OrderWithDetailsResponse>.CreateSuccess(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<Order>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            var created = await _services.OrderService.CreateOrderAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<Order>.CreateSuccess(created));
        }

        [HttpPost("with-details")]
        public async Task<IActionResult> CreateWithDetails([FromBody] OrderWithDetailsCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<OrderWithDetailsResponse>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            var created = await _services.OrderService.CreateOrderWithDetailsAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<OrderWithDetailsResponse>.CreateSuccess(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrderUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<OrderResponse>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            var updated = await _services.OrderService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(ApiResponse<OrderResponse>.CreateFail("Không tìm thấy đơn hàng", null, 404));
            return Ok(ApiResponse<OrderResponse>.CreateSuccess(updated));
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await _services.OrderService.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<OrderResponse>.CreateFail("Không tìm thấy đơn hàng", null, 404));
            return Ok(ApiResponse<OrderResponse>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _services.OrderService.DeleteAsync(id);
            if (!deleted) return NotFound(ApiResponse<string>.CreateFail("Không tìm thấy đơn hàng", null, 404));
            return Ok(ApiResponse<string>.CreateSuccess("Đã xóa"));
        }


        [HttpPost("{orderId}/deposits/preorder")]
        public async Task<IActionResult> CreatePreOrderDeposit(Guid orderId, [FromBody] PreOrderDepositRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<string>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            dto.OrderId = orderId;

            try
            {
                var result = await _services.DepositService.CreatePreOrderDepositAsync(dto);
                return Ok(ApiResponse<DepositResponse>.CreateSuccess(result, "Tạo đặt cọc đơn hàng trước thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.CreateFail(ex.Message, null, 400));
            }
        }

        [HttpPost("{orderId}/request-approval")]
        public async Task<IActionResult> RequestDealerManagerApproval(Guid orderId, [FromBody] DealerManagerApprovalRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<string>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            var result = await _services.OrderService.RequestDealerManagerApprovalAsync(orderId, dto);
            
            if (!result.Success)
            {
                return BadRequest(ApiResponse<OrderFlowResponseDto>.CreateFail(result.Message, null, 400));
            }

            return Ok(ApiResponse<OrderFlowResponseDto>.CreateSuccess(result, result.Message));
        }

        [HttpPost("{orderId}/approve-by-manager")]
        public async Task<IActionResult> ApproveDealerOrderRequest(Guid orderId, [FromQuery] Guid approvedByUserId)
        {
            if (approvedByUserId == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.CreateFail("ApprovedByUserId là bắt buộc", null, 400));
            }

            var result = await _services.OrderService.ApproveDealerOrderRequestAsync(orderId, approvedByUserId);
            
            if (!result.Success)
            {
                return BadRequest(ApiResponse<OrderFlowResponseDto>.CreateFail(result.Message, null, 400));
            }

            return Ok(ApiResponse<OrderFlowResponseDto>.CreateSuccess(result, result.Message));
        }

        [HttpPost("{orderId}/notify-customer")]
        public async Task<IActionResult> NotifyCustomer(Guid orderId, [FromBody] CustomerNotificationRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<string>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            var success = await _services.OrderService.NotifyCustomerAsync(orderId, dto);
            
            if (!success)
            {
                return BadRequest(ApiResponse<string>.CreateFail("Không thể thông báo cho khách hàng. Không tìm thấy đơn hàng hoặc khách hàng.", null, 400));
            }

            return Ok(ApiResponse<string>.CreateSuccess("Đã thông báo khách hàng thành công"));
        }

        [HttpPatch("{orderId}/customer-confirmation")]
        public async Task<IActionResult> UpdateCustomerConfirmation(Guid orderId, [FromBody] CustomerConfirmationRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<string>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            try
            {
                var result = await _services.OrderService.UpdateCustomerConfirmationAsync(orderId, dto);
                var message = dto.IsConfirmed ? "Đơn hàng đã được xác nhận bởi khách hàng" : "Đơn hàng đã bị từ chối bởi khách hàng";
                return Ok(ApiResponse<OrderResponse>.CreateSuccess(result, message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.CreateFail(ex.Message, null, 400));
            }
        }

        [HttpPost("{orderId}/confirm-payment")]
        public async Task<IActionResult> ConfirmPayment(Guid orderId, [FromBody] ConfirmPaymentRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<string>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            var result = await _services.OrderService.ConfirmPaymentAsync(orderId, dto);
            
            if (!result.Success)
            {
                return BadRequest(ApiResponse<OrderFlowResponseDto>.CreateFail(result.Message, null, 400));
            }

            return Ok(ApiResponse<OrderFlowResponseDto>.CreateSuccess(result, result.Message));
        }

        [HttpPost("{orderId}/handover")]
        public async Task<IActionResult> CreateHandover(Guid orderId, [FromBody] OrderHandoverRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<string>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            try
            {
                var result = await _services.HandoverRecordService.CreateHandoverWithVehicleAssignmentAsync(orderId, dto);
                return Ok(ApiResponse<HandoverRecordResponseDto>.CreateSuccess(result, "Hoàn thành bàn giao thành công. Đơn hàng đã được đánh dấu là HOÀN TẤT."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.CreateFail(ex.Message, null, 400));
            }
        }
    }
}
