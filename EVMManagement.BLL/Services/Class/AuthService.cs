using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using EVMManagement.BLL.DTOs.Request.Auth;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Auth;
using EVMManagement.BLL.Options;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.UnitOfWork;

namespace EVMManagement.BLL.Services.Class
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;
        private readonly PasswordHasher<Account> _passwordHasher = new();

        public AuthService(IUnitOfWork unitOfWork, IDistributedCache cache, IOptions<JwtSettings> jwtOptions, ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _jwtSettings = jwtOptions.Value;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginTokenDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return ApiResponse<LoginTokenDto>.CreateFail("Yêu cầu không hợp lệ.");
            }

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return ApiResponse<LoginTokenDto>.CreateFail("Email và mật khẩu là bắt buộc.");
            }

            var account = await _unitOfWork.Accounts.GetByEmailAsync(request.Email);
            if (account == null)
            {
                return ApiResponse<LoginTokenDto>.CreateFail("Email hoặc mật khẩu không đúng.", errorCode: 401);
            }

            if (account.IsDeleted)
            {
                return ApiResponse<LoginTokenDto>.CreateFail("Tài khoản đã bị vô hiệu.", errorCode: 403);
            }

            if (!account.IsActive)
            {
                return ApiResponse<LoginTokenDto>.CreateFail("Tài khoản chưa được kích hoạt.", errorCode: 403);
            }

            var verifyResult = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, request.Password);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                return ApiResponse<LoginTokenDto>.CreateFail("Email hoặc mật khẩu không đúng.", errorCode: 401);
            }

            if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
            {
                _logger.LogError("Jwt SecretKey không được cấu hình.");
                return ApiResponse<LoginTokenDto>.CreateFail("Lỗi cấu hình xác thực.", errorCode: 500);
            }

            var tokens = BuildLoginTokens(account);
            await StoreRefreshTokenAsync(account, tokens.RefreshToken, cancellationToken);

            return ApiResponse<LoginTokenDto>.CreateSuccess(tokens, "Đăng nhập thành công.");
        }

        private LoginTokenDto BuildLoginTokens(Account account)
        {
            var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);
            var accessToken = GenerateJwtToken(account, accessTokenExpiresAt);
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);

            return new LoginTokenDto
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshTokenExpiresAt
            };
        }

        private string GenerateJwtToken(Account account, DateTime expiresAt)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("Id", account.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, account.Email),
                new Claim("Email", account.Email),
                new Claim(ClaimTypes.Role, account.Role.ToString()),
                new Claim("Role", account.Role.ToString()),
                new Claim("IsActive", account.IsActive.ToString()),
                new Claim("IsDeleted", account.IsDeleted.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            Span<byte> buffer = stackalloc byte[32];
            RandomNumberGenerator.Fill(buffer);
            return Convert.ToHexString(buffer);
        }

        private async Task StoreRefreshTokenAsync(Account account, string refreshToken, CancellationToken cancellationToken)
        {
            var cacheKey = $"auth:refresh:{refreshToken}";
            var payload = account.Id.ToString();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(_jwtSettings.RefreshTokenExpiryDays)
            };

            await _cache.SetStringAsync(cacheKey, payload, options, cancellationToken);
        }
    }
}
