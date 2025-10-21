using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.DigitalSignature;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.DigitalSignature;
using EVMManagement.API.Services;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DigitalSignaturesController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public DigitalSignaturesController(IServiceFacade services)
        {
            _services = services;
        }

        [HttpPost("request-otp")]
        public async Task<IActionResult> RequestOtp([FromBody] RequestOtpDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<OtpRequestResponse>.CreateFail("Validation failed", errors, 400));
            }

            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                var result = await _services.DigitalSignatureService.RequestOtpAsync(dto, ipAddress, userAgent);
                return Ok(ApiResponse<OtpRequestResponse>.CreateSuccess(result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<OtpRequestResponse>.CreateFail(ex.Message, null, 400));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OtpRequestResponse>.CreateFail("Internal server error", new List<string> { ex.Message }, 500));
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<DigitalSignatureResponse>.CreateFail("Validation failed", errors, 400));
            }

            try
            {
                var result = await _services.DigitalSignatureService.VerifyOtpAsync(dto);
                return Ok(ApiResponse<DigitalSignatureResponse>.CreateSuccess(result));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<DigitalSignatureResponse>.CreateFail(ex.Message, null, 400));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<DigitalSignatureResponse>.CreateFail("Internal server error", new List<string> { ex.Message }, 500));
            }
        }

        [HttpPost("complete")]
        public async Task<IActionResult> CompleteSignature([FromBody] CompleteSignatureDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<DigitalSignatureResponse>.CreateFail("Validation failed", errors, 400));
            }

            try
            {
                var result = await _services.DigitalSignatureService.CompleteSignatureAsync(dto);
                return Ok(ApiResponse<DigitalSignatureResponse>.CreateSuccess(result));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<DigitalSignatureResponse>.CreateFail(ex.Message, null, 400));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<DigitalSignatureResponse>.CreateFail("Internal server error", new List<string> { ex.Message }, 500));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var item = await _services.DigitalSignatureService.GetByIdAsync(id);
                if (item == null) return NotFound(ApiResponse<DigitalSignatureResponse>.CreateFail("Digital signature not found", null, 404));
                return Ok(ApiResponse<DigitalSignatureResponse>.CreateSuccess(item));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<DigitalSignatureResponse>.CreateFail("Internal server error", new List<string> { ex.Message }, 500));
            }
        }

        [HttpGet("contracts/{contractId}")]
        public async Task<IActionResult> GetByContractId(Guid contractId)
        {
            try
            {
                var items = await _services.DigitalSignatureService.GetByContractIdAsync(contractId);
                return Ok(ApiResponse<List<DigitalSignatureResponse>>.CreateSuccess(items));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<DigitalSignatureResponse>>.CreateFail("Internal server error", new List<string> { ex.Message }, 500));
            }
        }

        [HttpGet("handover-records/{handoverRecordId}")]
        public async Task<IActionResult> GetByHandoverRecordId(Guid handoverRecordId)
        {
            try
            {
                var items = await _services.DigitalSignatureService.GetByHandoverRecordIdAsync(handoverRecordId);
                return Ok(ApiResponse<List<DigitalSignatureResponse>>.CreateSuccess(items));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<DigitalSignatureResponse>>.CreateFail("Internal server error", new List<string> { ex.Message }, 500));
            }
        }

        [HttpGet("dealer-contracts/{dealerContractId}")]
        public async Task<IActionResult> GetByDealerContractId(Guid dealerContractId)
        {
            try
            {
                var items = await _services.DigitalSignatureService.GetByDealerContractIdAsync(dealerContractId);
                return Ok(ApiResponse<List<DigitalSignatureResponse>>.CreateSuccess(items));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<DigitalSignatureResponse>>.CreateFail("Internal server error", new List<string> { ex.Message }, 500));
            }
        }
    }
}
