using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.Auth;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public AuthController(IServiceFacade services)
        {
            _services = services;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _services.AuthService.LoginAsync(request, cancellationToken);
            if (result.Success)
            {
                return Ok(result);
            }

            var statusCode = result.ErrorCode ?? StatusCodes.Status400BadRequest;
            if (statusCode == StatusCodes.Status401Unauthorized)
            {
                return Unauthorized(result);
            }

            if (statusCode == StatusCodes.Status403Forbidden)
            {
                return StatusCode(StatusCodes.Status403Forbidden, result);
            }

            return StatusCode(statusCode, result);
        }

        [HttpPost("register-dealer")]
        [Authorize(Roles = "EVM_ADMIN,DEALER_MANAGER")]
        public async Task<IActionResult> RegisterDealer([FromBody] RegisterDealerRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _services.AuthService.RegisterDealerAsync(request, cancellationToken);
            if (result.Success)
            {
                return Ok(result);
            }

            var statusCode = result.ErrorCode ?? StatusCodes.Status400BadRequest;
            if (statusCode == StatusCodes.Status409Conflict)
            {
                return Conflict(result);
            }

            return StatusCode(statusCode, result);
        }

        [HttpPost("accounts")]
        [Authorize(Roles = "EVM_ADMIN")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _services.AuthService.CreateAccountAsync(request, cancellationToken);
            if (result.Success)
            {
                return Ok(result);
            }

            var statusCode = result.ErrorCode ?? StatusCodes.Status400BadRequest;
            if (statusCode == StatusCodes.Status409Conflict)
            {
                return Conflict(result);
            }

            return StatusCode(statusCode, result);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _services.AuthService.RefreshTokenAsync(request, cancellationToken);
            if (result.Success)
            {
                return Ok(result);
            }

            var statusCode = result.ErrorCode ?? StatusCodes.Status400BadRequest;
            if (statusCode == StatusCodes.Status401Unauthorized)
            {
                return Unauthorized(result);
            }

            if (statusCode == StatusCodes.Status403Forbidden)
            {
                return StatusCode(StatusCodes.Status403Forbidden, result);
            }

            return StatusCode(statusCode, result);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _services.AuthService.ForgotPasswordAsync(request, cancellationToken);
            if (result.Success)
            {
                return Ok(result);
            }

            var statusCode = result.ErrorCode ?? StatusCodes.Status400BadRequest;
            return StatusCode(statusCode, result);
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _services.AuthService.ResetPasswordAsync(request, cancellationToken);
            if (result.Success)
            {
                return Ok(result);
            }

            var statusCode = result.ErrorCode ?? StatusCodes.Status400BadRequest;
            if (statusCode == StatusCodes.Status403Forbidden)
            {
                return StatusCode(StatusCodes.Status403Forbidden, result);
            }

            return StatusCode(statusCode, result);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request, CancellationToken cancellationToken)
        {
            var accountIdClaim = User.FindFirst("Id")?.Value;
            if (string.IsNullOrWhiteSpace(accountIdClaim) || !Guid.TryParse(accountIdClaim, out var accountId))
            {
                return Unauthorized(ApiResponse<string>.CreateFail("Không tìm thấy thông tin tài khoản.", errorCode: 401));
            }

            var result = await _services.AuthService.ChangePasswordAsync(accountId, request, cancellationToken);
            if (result.Success)
            {
                return Ok(result);
            }

            var statusCode = result.ErrorCode ?? StatusCodes.Status400BadRequest;
            if (statusCode == StatusCodes.Status401Unauthorized)
            {
                return Unauthorized(result);
            }

            if (statusCode == StatusCodes.Status403Forbidden)
            {
                return StatusCode(StatusCodes.Status403Forbidden, result);
            }

            if (statusCode == StatusCodes.Status404NotFound)
            {
                return NotFound(result);
            }

            return StatusCode(statusCode, result);
        }
    }
}
