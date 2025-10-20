using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Payment;
using EVMManagement.BLL.DTOs.Response.Payment;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IVnPayService
    {
        Task<VnPayPaymentResponse> CreatePaymentUrlAsync(VnPayPaymentRequest request, string ipAddress);
        Task<VnPayCallbackResponse> ProcessCallbackAsync(Dictionary<string, string> vnpayData);
        VnPayCallbackResponse ProcessReturnUrl(Dictionary<string, string> vnpayData);
    }
}
