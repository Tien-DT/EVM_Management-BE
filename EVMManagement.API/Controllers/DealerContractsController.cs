using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.BLL.DTOs.Request.DealerContract;
using EVMManagement.BLL.DTOs.Response.DealerContract;
using EVMManagement.BLL.DTOs.Response;
using System;
using System.Linq;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    
    public class DealerContractsController : ControllerBase
    {
        private readonly IDealerContractService _service;
        private readonly EVMManagement.BLL.Services.Interface.IUserProfileService _userProfileService;

        public DealerContractsController(IDealerContractService service, EVMManagement.BLL.Services.Interface.IUserProfileService userProfileService)
        {
            _service = service;
            _userProfileService = userProfileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));

            var res = await _service.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<DealerContractResponseDto>>.CreateSuccess(res));
        }

    [HttpGet("dealer/{dealerId}")]
    public async Task<IActionResult> GetByDealerId(Guid dealerId)
        {
            var item = await _service.GetByDealerIdAsync(dealerId);
            if (item == null) return NotFound(ApiResponse<DealerContractResponseDto>.CreateFail("DealerContract not found for dealer", null, 404));
            return Ok(ApiResponse<DealerContractResponseDto>.CreateSuccess(item));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<DealerContractResponseDto>.CreateFail("DealerContract not found", null, 404));
            return Ok(ApiResponse<DealerContractResponseDto>.CreateSuccess(item));
        }

        [HttpPost]
        [Authorize(Roles = "EVM_ADMIN")]
        public async Task<IActionResult> Create([FromBody] DealerContractCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<DealerContractResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            // If caller is EVM_ADMIN, allow create-and-sign by passing account id
            Guid? evmSignerAccountId = null;
            var accountIdClaim = User.FindFirst("Id")?.Value;
            if (User.IsInRole("EVM_ADMIN") && !string.IsNullOrWhiteSpace(accountIdClaim) && Guid.TryParse(accountIdClaim, out var aid))
            {
                evmSignerAccountId = aid;
            }

            var created = await _service.CreateAsync(dto, evmSignerAccountId, signAsEvm: evmSignerAccountId.HasValue);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<DealerContractResponseDto>.CreateSuccess(created));
        }

        [HttpPost("send-otp")]
        [Authorize]
        public async Task<IActionResult> SendOtp()
        {
            // Read account id and email from JWT claims
            var accountIdClaim = User.FindFirst("Id")?.Value;
            string? emailClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value
                                 ?? User.FindFirst("Email")?.Value;

            if (string.IsNullOrWhiteSpace(accountIdClaim) || !Guid.TryParse(accountIdClaim, out var accountId))
            {
                return Unauthorized(ApiResponse<string>.CreateFail("Account claim missing or invalid", null, 401));
            }

            // Resolve user's profile to get dealerId (if any)
            var profile = await _userProfileService.GetByAccountIdAsync(accountId);
            if (profile == null || !profile.DealerId.HasValue)
            {
                return BadRequest(ApiResponse<string>.CreateFail("User is not associated with a dealer", null, 400));
            }

            var dealerId = profile.DealerId.Value;

            var success = await _service.SendOtpAsync(dealerId, string.IsNullOrWhiteSpace(emailClaim) ? null : emailClaim);
            if (!success) return BadRequest(ApiResponse<string>.CreateFail("Failed to send OTP", null, 400));
            return Ok(ApiResponse<string>.CreateSuccess("OTP sent"));
        }

        [HttpPost("{dealerId}/verify-otp")]
        [Authorize]
        public async Task<IActionResult> VerifyOtp(Guid dealerId, [FromBody] EVMManagement.BLL.DTOs.Request.DealerContract.DealerOtpVerifyRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<string>.CreateFail("Invalid request", null, 400));

            // Try to get signer account id from JWT (if caller is authenticated)
            Guid? signerAccountId = null;
            var accountIdClaim = User.FindFirst("Id")?.Value;
            if (!string.IsNullOrWhiteSpace(accountIdClaim) && Guid.TryParse(accountIdClaim, out var accId))
            {
                signerAccountId = accId;
            }

            var valid = await _service.VerifyOtpAsync(dealerId, dto.Otp, signerAccountId);
            if (!valid) return BadRequest(ApiResponse<string>.CreateFail("OTP is invalid or expired", null, 400));

            return Ok(ApiResponse<string>.CreateSuccess("OTP verified"));
        }
    }
}
