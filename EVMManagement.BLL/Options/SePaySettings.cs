namespace EVMManagement.BLL.Options
{

    public class SePaySettings
    {
        public const string SectionName = "SePaySettings";
        public string MerchantCode { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0";
        public string SignatureType { get; set; } = "SHA256";
        public string Locale { get; set; } = "vi";
        public string Currency { get; set; } = "VND";
    }
}
