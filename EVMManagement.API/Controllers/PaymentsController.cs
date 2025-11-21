using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.Payment;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Payment;
using EVMManagement.API.Services;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

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
        [HttpPost("sepay/callback")]
        public async Task<IActionResult> SePayCallback()
        {
            try
            {
                Dictionary<string, string> sepayData;

                // Check if it's POST request with JSON body
                if (Request.Method == "POST" && Request.ContentType?.Contains("application/json") == true)
                {
                    using (var reader = new StreamReader(Request.Body))
                    {
                        var body = await reader.ReadToEndAsync();
                        Console.WriteLine($"[SEPay Webhook POST] Body: {body}");
                        
                        // Parse JSON body to Dictionary
                        var jsonData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(body);
                        sepayData = jsonData?.ToDictionary(x => x.Key, x => x.Value?.ToString() ?? string.Empty) 
                                    ?? new Dictionary<string, string>();
                    }
                }
                else
                {
                    // GET request or POST with form data - read from Query
                    Console.WriteLine($"[SEPay Webhook GET] Query: {Request.QueryString}");
                    sepayData = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
                }

                if (sepayData.Count == 0)
                {
                    Console.WriteLine("[SEPay Webhook] ERROR: No data received");
                    return BadRequest(ApiResponse<string>.CreateFail("No data received from SEPay", null, 400));
                }

                Console.WriteLine($"[SEPay Webhook] Processing data: {string.Join(", ", sepayData.Select(x => $"{x.Key}={x.Value}"))}");

                var result = await _services.SePayService.ProcessCallbackAsync(sepayData);

                if (result.Success)
                {
                    Console.WriteLine($"[SEPay Webhook] SUCCESS: {result.Message}");
                    // SEPay expects: {"success": true} with status 200 or 201
                    return Ok(new { success = true, message = "Payment processed successfully" });
                }
                else
                {
                    Console.WriteLine($"[SEPay Webhook] FAILED: {result.Message}");
                    // Return success=false but still 200 OK so SEPay knows we received it
                    return Ok(new { success = false, code = result.ResponseCode, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SEPay Webhook] EXCEPTION: {ex.Message}");
                Console.WriteLine($"[SEPay Webhook] StackTrace: {ex.StackTrace}");
                return Ok(new { success = false, code = "99", message = ex.Message });
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

        [HttpGet("sepay/status/{transactionCode}")]
        public async Task<IActionResult> CheckSePayTransactionStatus(string transactionCode)
        {
            try
            {
                var result = await _services.SePayService.CheckTransactionStatusAsync(transactionCode);
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
