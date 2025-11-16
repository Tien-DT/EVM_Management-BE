namespace EVMManagement.BLL.Options
{
    /// <summary>
    /// SEPay configuration - Cấu hình thực tế với tài khoản ngân hàng
    /// SEPay hoạt động qua chuyển khoản ngân hàng và kiểm tra webhook
    /// </summary>
    public class SePaySettings
    {
        public const string SectionName = "SePaySettings";
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string TransactionPrefix { get; set; } = string.Empty;
        public string ApiBaseUrl { get; set; } = "https://my.sepay.vn/userapi";
        public string QRApiUrl { get; set; } = "https://img.vietqr.io";
        public int DefaultExpiryMinutes { get; set; } = 15;
        public string ReturnUrl { get; set; } = string.Empty;
        public string Locale { get; set; } = "vi";
        public string Currency { get; set; } = "VND";
    }
}
