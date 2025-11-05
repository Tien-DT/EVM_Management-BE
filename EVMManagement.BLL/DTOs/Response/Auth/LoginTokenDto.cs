using System;

namespace EVMManagement.BLL.DTOs.Response.Auth
{
    public class LoginTokenDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiresAt { get; set; }
        public bool IsPasswordChange { get; set; }
    }
}
