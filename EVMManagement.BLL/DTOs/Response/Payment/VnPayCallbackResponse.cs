using System;

namespace EVMManagement.BLL.DTOs.Response.Payment
{

    public class VnPayCallbackResponse : PaymentCallbackResponse
    {

        public string VnpayTransactionNo 
        { 
            get => GatewayTransactionNo; 
            set => GatewayTransactionNo = value; 
        }
    }
}
