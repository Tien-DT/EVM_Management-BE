using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.DealerContract;
using EVMManagement.BLL.DTOs.Response.DealerContract;
using EVMManagement.BLL.DTOs.Response;
using System;
using System.Linq;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    
    public class DealerContractsController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public DealerContractsController(IServiceFacade services)
        {
            _services = services;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));

            var res = await _services.DealerContractService.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<DealerContractResponseDto>>.CreateSuccess(res));
        }

        [HttpGet("dealer/{dealerId}")]
        public async Task<IActionResult> GetByDealerId(Guid dealerId)
        {
            var item = await _services.DealerContractService.GetByDealerIdAsync(dealerId);
            if (item == null) return NotFound(ApiResponse<DealerContractResponseDto>.CreateFail("DealerContract not found for dealer", null, 404));
            return Ok(ApiResponse<DealerContractResponseDto>.CreateSuccess(item));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.DealerContractService.GetByIdAsync(id);
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

            
            Guid? evmSignerAccountId = null;
            var accountIdClaim = User.FindFirst("Id")?.Value;
            if (User.IsInRole("EVM_ADMIN") && !string.IsNullOrWhiteSpace(accountIdClaim) && Guid.TryParse(accountIdClaim, out var aid))
            {
                evmSignerAccountId = aid;
            }

            var created = await _services.DealerContractService.CreateAsync(dto, evmSignerAccountId, signAsEvm: evmSignerAccountId.HasValue);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<DealerContractResponseDto>.CreateSuccess(created));
        }

      

        [HttpPost("{dealerId}/verify-otp")]
        [Authorize]
        public async Task<IActionResult> VerifyOtp(Guid dealerId, [FromBody] DealerOtpVerifyRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<string>.CreateFail("Invalid request", null, 400));

            string? signerEmail = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value
                                 ?? User.FindFirst("Email")?.Value;

            if (string.IsNullOrWhiteSpace(signerEmail))
            {
                return BadRequest(ApiResponse<string>.CreateFail("Signer email claim is required for OTP verification", null, 400));
            }


            var marked = await _services.DealerContractService.MarkAsSignedAsync(dealerId, dto.Otp, signerEmail);
            if (!marked) return BadRequest(ApiResponse<string>.CreateFail("OTP is invalid, expired or failed to sign contract", null, 400));

            return Ok(ApiResponse<string>.CreateSuccess("OTP verified and contract signed"));
        }
    }
}
