using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace EVMManagement.BLL.Helpers
{

    public class SePayLibrary
    {
        private readonly SortedDictionary<string, string> _requestData = new SortedDictionary<string, string>();
        private readonly SortedDictionary<string, string> _responseData = new SortedDictionary<string, string>();

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

        public string CreateRequestUrl(string baseUrl, string secretKey, string signatureType = "SHA256")
        {
            var queryString = BuildQueryString(_requestData);
            var signData = BuildSignData(_requestData);
            
            var signature = signatureType.ToUpper() == "MD5" 
                ? CreateMD5Signature(secretKey, signData) 
                : CreateSHA256Signature(secretKey, signData);

            queryString += "&signature=" + signature;

            return baseUrl + "?" + queryString;
        }

        public bool ValidateSignature(string inputSignature, string secretKey, string signatureType = "SHA256")
        {
            var dataToSign = new SortedDictionary<string, string>(_responseData);
            
            dataToSign.Remove("signature");
            dataToSign.Remove("sig");
            
            var signData = BuildSignData(dataToSign);
            
            var calculatedSignature = signatureType.ToUpper() == "MD5"
                ? CreateMD5Signature(secretKey, signData)
                : CreateSHA256Signature(secretKey, signData);

            return calculatedSignature.Equals(inputSignature, StringComparison.InvariantCultureIgnoreCase);
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

        private string BuildSignData(IDictionary<string, string> data)
        {
            var signData = new StringBuilder();
            foreach (var kvp in data.Where(x => !string.IsNullOrEmpty(x.Value)))
            {
                signData.Append(kvp.Key)
                        .Append("=")
                        .Append(kvp.Value)
                        .Append("&");
            }

            if (signData.Length > 0)
            {
                signData.Remove(signData.Length - 1, 1);
            }

            return signData.ToString();
        }

        private string CreateSHA256Signature(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashValue = hmac.ComputeHash(dataBytes);
                return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
            }
        }

        private string CreateMD5Signature(string key, string data)
        {
            var signData = data + key;
            using (var md5 = MD5.Create())
            {
                var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(signData));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
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
