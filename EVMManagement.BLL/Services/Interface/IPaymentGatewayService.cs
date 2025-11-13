using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Payment;
using EVMManagement.BLL.DTOs.Response.Payment;

namespace EVMManagement.BLL.Services.Interface
{
    /// <summary>
    /// Interface chung cho tất cả payment gateway services
    /// Hỗ trợ VNPay, SEPay và các gateway khác trong tương lai
    /// </summary>
    public interface IPaymentGatewayService
    {
        Task<PaymentResponse> CreatePaymentUrlAsync(PaymentRequest request, string ipAddress);
        Task<PaymentCallbackResponse> ProcessCallbackAsync(Dictionary<string, string> callbackData);
        Task<PaymentCallbackResponse> ProcessReturnUrlAsync(Dictionary<string, string> returnData);
    }
}
