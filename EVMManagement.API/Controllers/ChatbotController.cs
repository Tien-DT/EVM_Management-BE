using System;
using System.Linq;
using System.Threading.Tasks;
using EVMManagement.API.Services;
using EVMManagement.BLL.DTOs.Request.Chatbot;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Chatbot;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ChatbotController : BaseController
    {
        public ChatbotController(IServiceFacade services) : base(services)
        {
        }

        [HttpPost("chat")]
        [Authorize]
        public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<ChatResponseDto>.CreateFail("Dữ liệu không hợp lệ", errors, 400));
            }

            try
            {
                var userRole = GetCurrentRole();
                if (!userRole.HasValue)
                {
                    return Unauthorized(ApiResponse<ChatResponseDto>.CreateFail(
                        "Không thể xác định quyền của bạn", 
                        null, 
                        401));
                }

                request.UserRole = userRole.Value;
                request.UserId = GetCurrentAccountId();

                if (userRole.Value == DAL.Models.Enums.AccountRole.DEALER_MANAGER || 
                    userRole.Value == DAL.Models.Enums.AccountRole.DEALER_STAFF)
                {
                    var dealerId = await GetCurrentUserDealerIdAsync();
                    if (dealerId.HasValue)
                    {
                        request.DealerId = dealerId.Value;
                    }
                }

                var response = await Services.ChatbotService.ProcessChatAsync(request);
                return Ok(ApiResponse<ChatResponseDto>.CreateSuccess(response));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<ChatResponseDto>.CreateFail(
                    ex.Message, 
                    null, 
                    403));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ChatResponseDto>.CreateFail(
                    $"Lỗi xử lý yêu cầu: {ex.Message}", 
                    null, 
                    500));
            }
        }
    }
}
