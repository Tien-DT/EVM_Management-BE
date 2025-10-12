using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.Auth;
using EVMManagement.BLL.Services.Interface;

namespace EVMManagement.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
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
    }
}
