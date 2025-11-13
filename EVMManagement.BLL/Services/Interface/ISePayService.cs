using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Payment;
using EVMManagement.BLL.DTOs.Response.Payment;

namespace EVMManagement.BLL.Services.Interface
{
    public interface ISePayService
    {
        Task<PaymentResponse> CreatePaymentUrlAsync(PaymentRequest request, string ipAddress);
        Task<PaymentCallbackResponse> ProcessCallbackAsync(Dictionary<string, string> callbackData);
        Task<PaymentCallbackResponse> ProcessReturnUrlAsync(Dictionary<string, string> returnData);
    }
}
