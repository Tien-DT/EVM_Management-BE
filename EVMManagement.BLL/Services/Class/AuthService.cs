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
using EVMManagement.DAL.Models.Enums;

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

        public async Task<ApiResponse<string>> RegisterDealerAsync(RegisterDealerRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                {
                    return ApiResponse<string>.CreateFail("Yêu cầu không hợp lệ.");
                }

                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return ApiResponse<string>.CreateFail("Email là bắt buộc.");
                }

                if (string.IsNullOrWhiteSpace(request.FullName))
                {
                    return ApiResponse<string>.CreateFail("Họ tên là bắt buộc.");
                }

                // Chỉ cho phép role đại lý
                if (request.Role != AccountRole.DEALER_MANAGER && request.Role != AccountRole.DEALER_STAFF)
                {
                    return ApiResponse<string>.CreateFail("Role không hợp lệ cho đại lý.");
                }

                var existed = await _unitOfWork.Accounts.GetByEmailAsync(request.Email);
                if (existed != null)
                {
                    return ApiResponse<string>.CreateFail("Email đã được sử dụng.", errorCode: 409);
                }

                var plainPassword = GenerateRandomPassword();

                var account = new Account
                {
                    Email = request.Email.Trim(),
                    IsActive = true,
                    Role = request.Role
                };
                account.PasswordHash = _passwordHasher.HashPassword(account, plainPassword);

                await _unitOfWork.Accounts.AddAsync(account);

                var profile = new UserProfile
                {
                    AccountId = account.Id,
                    DealerId = request.DealerId,
                    FullName = request.FullName.Trim(),
                    Phone = request.Phone,
                    CardId = request.CardId
                };
                await _unitOfWork.UserProfiles.AddAsync(profile);

                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<string>.CreateSuccess(plainPassword, "Tạo tài khoản đại lý thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng ký tài khoản đại lý");
                return ApiResponse<string>.CreateFail(ex);
            }
        }

        private static string GenerateRandomPassword(int length = 12)
        {
            const string allowed = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%^&*()_-+";
            Span<char> chars = stackalloc char[length];
            Span<byte> bytes = stackalloc byte[length];
            RandomNumberGenerator.Fill(bytes);
            for (int i = 0; i < length; i++)
            {
                chars[i] = allowed[bytes[i] % allowed.Length];
            }
            return new string(chars);
        }
    }
}
