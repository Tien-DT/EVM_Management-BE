using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.Payment;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Payment;
using EVMManagement.API.Services;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public PaymentsController(IServiceFacade services)
        {
            _services = services;
        }

        [HttpPost("vnpay/create")]
        public async Task<IActionResult> CreateVnPayPayment([FromBody] VnPayPaymentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<string>.CreateFail("Validation failed", errors, 400));
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                var result = await _services.VnPayService.CreatePaymentUrlAsync(request, ipAddress);

                return Ok(ApiResponse<VnPayPaymentResponse>.CreateSuccess(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.CreateFail(ex.Message, null, 400));
            }
        }

        [HttpGet("vnpay/callback")]
        public async Task<IActionResult> VnPayCallback()
        {
            try
            {
                var vnpayData = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

                if (vnpayData.Count == 0)
                {
                    return BadRequest(ApiResponse<string>.CreateFail("No data received from VNPay", null, 400));
                }

                var result = await _services.VnPayService.ProcessCallbackAsync(vnpayData);

                if (result.Success)
                {
                    return Ok(new { RspCode = "00", Message = "Confirm Success" });
                }
                else
                {
                    return Ok(new { RspCode = result.ResponseCode, Message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { RspCode = "99", Message = ex.Message });
            }
        }

        [HttpGet("vnpay/return")]
        public async Task<IActionResult> VnPayReturn()
        {
            try
            {
                var vnpayData = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

                if (vnpayData.Count == 0)
                {
                    return BadRequest(ApiResponse<string>.CreateFail("No data received from VNPay", null, 400));
                }

                var result = await _services.VnPayService.ProcessReturnUrlAsync(vnpayData);

                return Ok(ApiResponse<VnPayCallbackResponse>.CreateSuccess(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.CreateFail(ex.Message, null, 400));
            }
        }

        #region SEPay Endpoints

        [HttpPost("sepay/create")]
        public async Task<IActionResult> CreateSePayPayment([FromBody] PaymentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponse<string>.CreateFail("Validation failed", errors, 400));
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                var result = await _services.SePayService.CreatePaymentUrlAsync(request, ipAddress);

                return Ok(ApiResponse<PaymentResponse>.CreateSuccess(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.CreateFail(ex.Message, null, 400));
            }
        }

        [HttpGet("sepay/callback")]
        public async Task<IActionResult> SePayCallback()
        {
            try
            {
                var sepayData = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

                if (sepayData.Count == 0)
                {
                    return BadRequest(ApiResponse<string>.CreateFail("No data received from SEPay", null, 400));
                }

                var result = await _services.SePayService.ProcessCallbackAsync(sepayData);

                if (result.Success)
                {
                    return Ok(new { RspCode = "00", Message = "Confirm Success" });
                }
                else
                {
                    return Ok(new { RspCode = result.ResponseCode, Message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { RspCode = "99", Message = ex.Message });
            }
        }

        [HttpGet("sepay/return")]
        public async Task<IActionResult> SePayReturn()
        {
            try
            {
                var sepayData = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

                if (sepayData.Count == 0)
                {
                    return BadRequest(ApiResponse<string>.CreateFail("No data received from SEPay", null, 400));
                }

                var result = await _services.SePayService.ProcessReturnUrlAsync(sepayData);

                return Ok(ApiResponse<PaymentCallbackResponse>.CreateSuccess(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.CreateFail(ex.Message, null, 400));
            }
        }

        #endregion
    }
}
