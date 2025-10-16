namespace EVMManagement.BLL.Options
{
    public class GmailApiSettings
    {
        public const string SectionName = "GmailApiSettings";
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }
}
