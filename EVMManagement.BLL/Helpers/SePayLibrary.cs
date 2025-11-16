using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Security.Cryptography;

namespace EVMManagement.BLL.Helpers
{
    /// <summary>
    /// SEPay Helper - Hỗ trợ tạo QR code và xử lý webhook từ SEPay
    /// SEPay hoạt động bằng cách tạo QR VietQR, khách hàng quét mã và chuyển khoản
    /// </summary>
    public class SePayLibrary
    {
        private readonly Dictionary<string, string> _requestData = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _responseData = new Dictionary<string, string>();

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData[key] = value;
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData[key] = value;
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var value) ? value : string.Empty;
        }

        /// <summary>
        /// Tạo URL QR code VietQR cho khách hàng quét và thanh toán
        /// </summary>
        public string CreateQRCodeUrl(
            string qrApiUrl,
            string accountNumber,
            string accountName,
            string bankCode,
            decimal amount,
            string transactionContent,
            string template = "compact2")
        {
            var encodedContent = WebUtility.UrlEncode(transactionContent);
            var encodedAccountName = WebUtility.UrlEncode(accountName);
            
            return $"{qrApiUrl}/image/{bankCode}-{accountNumber}-{template}.png?amount={amount}&addInfo={encodedContent}&accountName={encodedAccountName}";
        }

        /// <summary>
        /// Tạo nội dung chuyển khoản với mã giao dịch
        /// Format đơn giản: EVM{timestamp} - Ví dụ: EVM20251113221731
        /// </summary>
        public string CreateTransactionContent(string prefix, string transactionCode, string orderInfo = "")
        {
            // Chỉ trả về transaction code đơn giản, không thêm prefix hay orderInfo
            return transactionCode;
        }

        /// <summary>
        /// Validate webhook signature từ SEPay (nếu có webhook secret)
        /// </summary>
        public bool ValidateWebhookSignature(string payload, string signature, string webhookSecret)
        {
            if (string.IsNullOrEmpty(webhookSecret))
            {
                return true; // Không yêu cầu validate nếu không có secret
            }

            var calculatedSignature = CreateHmacSHA256(payload, webhookSecret);
            return calculatedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
        }

        private string CreateHmacSHA256(string data, string key)
        {
            var encoding = new UTF8Encoding();
            var keyBytes = encoding.GetBytes(key);
            var dataBytes = encoding.GetBytes(data);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(dataBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public string CreateRequestUrl(string baseUrl)
        {
            var queryString = BuildQueryString(_requestData);
            return baseUrl + "?" + queryString;
        }

        private string BuildQueryString(IDictionary<string, string> data)
        {
            var query = new StringBuilder();
            foreach (var kvp in data.Where(x => !string.IsNullOrEmpty(x.Value)))
            {
                query.Append(WebUtility.UrlEncode(kvp.Key))
                     .Append("=")
                     .Append(WebUtility.UrlEncode(kvp.Value))
                     .Append("&");
            }

            if (query.Length > 0)
            {
                query.Remove(query.Length - 1, 1);
            }

            return query.ToString();
        }

        public void ParseResponseData(Dictionary<string, string> queryData)
        {
            foreach (var kvp in queryData)
            {
                AddResponseData(kvp.Key, kvp.Value);
            }
        }
    }
}
