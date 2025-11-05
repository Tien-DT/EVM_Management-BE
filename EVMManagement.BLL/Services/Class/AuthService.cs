using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using EVMManagement.BLL.DTOs.Request.Auth;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Auth;
using EVMManagement.BLL.Options;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.BLL.Services.Templates;
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
        private readonly IEmailService _emailService;
        private readonly PasswordHasher<Account> _passwordHasher = new();

        public AuthService(IUnitOfWork unitOfWork, IDistributedCache cache, IOptions<JwtSettings> jwtOptions, ILogger<AuthService> logger, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _jwtSettings = jwtOptions.Value;
            _logger = logger;
            _emailService = emailService;
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
                RefreshTokenExpiresAt = refreshTokenExpiresAt,
                IsPasswordChange = account.IsPasswordChange
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

        private static string GenerateResetPasswordCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ0123456789";
            Span<char> code = stackalloc char[6];
            Span<byte> bytes = stackalloc byte[6];
            RandomNumberGenerator.Fill(bytes);
            for (int i = 0; i < 6; i++)
            {
                code[i] = chars[bytes[i] % chars.Length];
            }
            return new string(code);
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

        public async Task<ApiResponse<string>> RegisterDealerAsync(RegisterDealerRequestDto request, AccountRole currentUserRole, Guid? currentUserDealerId = null, CancellationToken cancellationToken = default)
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

                var normalizedEmail = request.Email.Trim();
                var normalizedFullName = request.FullName.Trim();
                var phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
                var cardId = string.IsNullOrWhiteSpace(request.CardId) ? null : request.CardId.Trim();

                // Chỉ cho phép role đại lý
                if (request.Role != AccountRole.DEALER_MANAGER && request.Role != AccountRole.DEALER_STAFF)
                {
                    return ApiResponse<string>.CreateFail("Role không hợp lệ cho đại lý.");
                }

                // validate DealerId dựa trên role của user hiện tại
                if (currentUserRole == AccountRole.DEALER_MANAGER)
                {
                    if (!currentUserDealerId.HasValue)
                    {
                        _logger.LogWarning("DEALER_MANAGER không có DealerId trong UserProfile. Email: {Email}", request.Email);
                        return ApiResponse<string>.CreateFail("Không tìm thấy thông tin dealer của bạn. Vui lòng liên hệ quản trị viên để được hỗ trợ.", errorCode: 403);
                    }

                    if (request.DealerId != currentUserDealerId.Value)
                    {
                        _logger.LogWarning("DEALER_MANAGER {DealerManagerDealerId} cố tạo account cho dealer khác {RequestDealerId}", 
                            currentUserDealerId.Value, request.DealerId);
                        return ApiResponse<string>.CreateFail($"Bạn chỉ có thể tạo tài khoản cho dealer của mình. DealerId của bạn: {currentUserDealerId.Value}", errorCode: 403);
                    }
                }

                var dealer = await _unitOfWork.Dealers.GetByIdAsync(request.DealerId);
                if (dealer == null)
                {
                    _logger.LogWarning("Cố tạo account cho dealer không tồn tại. DealerId: {DealerId}", request.DealerId);
                    return ApiResponse<string>.CreateFail($"Dealer với ID '{request.DealerId}' không tồn tại trong hệ thống.", errorCode: 404);
                }

                if (dealer.IsDeleted)
                {
                    _logger.LogWarning("Cố tạo account cho dealer đã bị xóa. DealerId: {DealerId}, DealerName: {DealerName}", 
                        request.DealerId, dealer.Name);
                    return ApiResponse<string>.CreateFail($"Dealer '{dealer.Name}' đã bị xóa. Vui lòng liên hệ quản trị viên.", errorCode: 400);
                }

                if (!dealer.IsActive)
                {
                    _logger.LogWarning("Cố tạo account cho dealer chưa active. DealerId: {DealerId}, DealerName: {DealerName}", 
                        request.DealerId, dealer.Name);
                    return ApiResponse<string>.CreateFail($"Dealer '{dealer.Name}' chưa được kích hoạt. Vui lòng liên hệ quản trị viên.", errorCode: 400);
                }

                if (phone != null)
                {
                    if (!Regex.IsMatch(phone, @"^\d{10}$"))
                    {
                        return ApiResponse<string>.CreateFail("Số điện thoại phải gồm đúng 10 chữ số.");
                    }

                    var phoneExists = await _unitOfWork.UserProfiles
                        .GetAllAsync()
                        .AnyAsync(u => !u.IsDeleted && u.Phone == phone && u.Account != null && !u.Account.IsDeleted);
                    if (phoneExists)
                    {
                        return ApiResponse<string>.CreateFail($"Số điện thoại '{phone}' đã được sử dụng.", errorCode: 409);
                    }
                }

                if (cardId != null)
                {
                    if (!Regex.IsMatch(cardId, @"^\d{12}$"))
                    {
                        return ApiResponse<string>.CreateFail("Căn cước phải gồm đúng 12 chữ số.");
                    }

                    var cardExists = await _unitOfWork.UserProfiles
                        .GetAllAsync()
                        .AnyAsync(u => !u.IsDeleted && u.CardId == cardId && u.Account != null && !u.Account.IsDeleted);
                    if (cardExists)
                    {
                        return ApiResponse<string>.CreateFail($"Căn cước '{cardId}' đã được sử dụng.", errorCode: 409);
                    }
                }

                var dealerProfileExists = await _unitOfWork.UserProfiles
                    .GetAllAsync()
                    .AnyAsync(u => !u.IsDeleted && u.DealerId == request.DealerId && u.Account != null && !u.Account.IsDeleted);
                if (dealerProfileExists)
                {
                    return ApiResponse<string>.CreateFail("Dealer này đã có tài khoản trong hệ thống.", errorCode: 409);
                }

                var existed = await _unitOfWork.Accounts.GetByEmailAsync(normalizedEmail);
                if (existed != null)
                {
                    _logger.LogWarning("Cố tạo account với email đã tồn tại. Email: {Email}", normalizedEmail);
                    return ApiResponse<string>.CreateFail($"Email '{normalizedEmail}' đã được sử dụng. Vui lòng sử dụng email khác.", errorCode: 409);
                }

                var plainPassword = GenerateRandomPassword();

                var account = new Account
                {
                    Email = normalizedEmail,
                    IsActive = true,
                    Role = request.Role,
                    IsPasswordChange = false
                };
                account.PasswordHash = _passwordHasher.HashPassword(account, plainPassword);

                await _unitOfWork.Accounts.AddAsync(account);

                var profile = new UserProfile
                {
                    AccountId = account.Id,
                    DealerId = request.DealerId,
                    FullName = normalizedFullName,
                    Phone = phone,
                    CardId = cardId
                };
                await _unitOfWork.UserProfiles.AddAsync(profile);

                await _unitOfWork.SaveChangesAsync();

                try
                {
                    var emailBody = EmailTemplates.WelcomeDealerEmail(normalizedFullName, normalizedEmail, plainPassword);
                    await _emailService.SendEmailAsync(normalizedEmail, EmailTemplates.Subjects.WelcomeDealer, emailBody, isHtml: true);
                    _logger.LogInformation("Email chào mừng đã được gửi đến {Email} cho account mới tạo", normalizedEmail);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Lỗi khi gửi email chào mừng đến {Email}. Account đã được tạo nhưng email không gửi được.", normalizedEmail);
                }

                _logger.LogInformation("Tài khoản dealer {Role} được tạo thành công. CreatedBy: {CurrentUserRole}, AccountId: {AccountId}, DealerId: {DealerId}, Email: {Email}, DealerName: {DealerName}", 
                    request.Role, currentUserRole, account.Id, request.DealerId, normalizedEmail, dealer.Name);

                return ApiResponse<string>.CreateSuccess(account.Id.ToString(), $"Tạo tài khoản cho '{normalizedFullName}' thành công. Thông tin đăng nhập đã được gửi qua email.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi nghiêm trọng khi đăng ký tài khoản dealer. CurrentUserRole: {CurrentUserRole}, DealerId: {DealerId}, Email: {Email}, FullName: {FullName}", 
                    currentUserRole, request?.DealerId, request?.Email, request?.FullName);
                return ApiResponse<string>.CreateFail("Có lỗi xảy ra khi tạo tài khoản. Vui lòng thử lại sau hoặc liên hệ quản trị viên.", errorCode: 500);
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

        public async Task<ApiResponse<string>> CreateAccountAsync(CreateAccountRequestDto request, CancellationToken cancellationToken = default)
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

                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return ApiResponse<string>.CreateFail("Mật khẩu là bắt buộc.");
                }

                if (string.IsNullOrWhiteSpace(request.FullName))
                {
                    return ApiResponse<string>.CreateFail("Họ tên là bắt buộc.");
                }

                var normalizedEmail = request.Email.Trim();
                var normalizedFullName = request.FullName.Trim();
                var phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
                var cardId = string.IsNullOrWhiteSpace(request.CardId) ? null : request.CardId.Trim();

                if (phone != null)
                {
                    if (!Regex.IsMatch(phone, @"^\d{10}$"))
                    {
                        return ApiResponse<string>.CreateFail("Số điện thoại phải gồm đúng 10 chữ số.");
                    }

                    var phoneExists = await _unitOfWork.UserProfiles
                        .GetAllAsync()
                        .AnyAsync(u => !u.IsDeleted && u.Phone == phone && u.Account != null && !u.Account.IsDeleted);
                    if (phoneExists)
                    {
                        return ApiResponse<string>.CreateFail($"Số điện thoại '{phone}' đã được sử dụng.", errorCode: 409);
                    }
                }

                if (cardId != null)
                {
                    if (!Regex.IsMatch(cardId, @"^\d{12}$"))
                    {
                        return ApiResponse<string>.CreateFail("Căn cước phải gồm đúng 12 chữ số.");
                    }

                    var cardExists = await _unitOfWork.UserProfiles
                        .GetAllAsync()
                        .AnyAsync(u => !u.IsDeleted && u.CardId == cardId && u.Account != null && !u.Account.IsDeleted);
                    if (cardExists)
                    {
                        return ApiResponse<string>.CreateFail($"Căn cước '{cardId}' đã được sử dụng.", errorCode: 409);
                    }
                }

                var existed = await _unitOfWork.Accounts.GetByEmailAsync(normalizedEmail);
                if (existed != null)
                {
                    return ApiResponse<string>.CreateFail("Email đã được sử dụng.", errorCode: 409);
                }

                var account = new Account
                {
                    Email = normalizedEmail,
                    IsActive = true,
                    Role = request.Role,
                    IsPasswordChange = true
                };
                account.PasswordHash = _passwordHasher.HashPassword(account, request.Password);

                await _unitOfWork.Accounts.AddAsync(account);

                var profile = new UserProfile
                {
                    AccountId = account.Id,
                    DealerId = null,
                    FullName = normalizedFullName,
                    Phone = phone,
                    CardId = cardId
                };
                await _unitOfWork.UserProfiles.AddAsync(profile);

                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<string>.CreateSuccess(account.Id.ToString(), "Tạo tài khoản thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo tài khoản");
                return ApiResponse<string>.CreateFail(ex);
            }
        }

        public async Task<ApiResponse<LoginTokenDto>> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return ApiResponse<LoginTokenDto>.CreateFail("Refresh token là bắt buộc.", errorCode: 401);
                }

                var cacheKey = $"auth:refresh:{request.RefreshToken}";
                var accountIdStr = await _cache.GetStringAsync(cacheKey, cancellationToken);

                if (string.IsNullOrWhiteSpace(accountIdStr))
                {
                    return ApiResponse<LoginTokenDto>.CreateFail("Refresh token không hợp lệ hoặc đã hết hạn.", errorCode: 401);
                }

                if (!Guid.TryParse(accountIdStr, out var accountId))
                {
                    return ApiResponse<LoginTokenDto>.CreateFail("Refresh token không hợp lệ.", errorCode: 401);
                }

                var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
                if (account == null)
                {
                    return ApiResponse<LoginTokenDto>.CreateFail("Tài khoản không tồn tại.", errorCode: 401);
                }

                if (account.IsDeleted)
                {
                    return ApiResponse<LoginTokenDto>.CreateFail("Tài khoản đã bị vô hiệu.", errorCode: 403);
                }

                if (!account.IsActive)
                {
                    return ApiResponse<LoginTokenDto>.CreateFail("Tài khoản chưa được kích hoạt.", errorCode: 403);
                }

                await _cache.RemoveAsync(cacheKey, cancellationToken);

                var tokens = BuildLoginTokens(account);
                await StoreRefreshTokenAsync(account, tokens.RefreshToken, cancellationToken);

                return ApiResponse<LoginTokenDto>.CreateSuccess(tokens, "Làm mới token thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi làm mới token");
                return ApiResponse<LoginTokenDto>.CreateFail(ex);
            }
        }

        public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email))
                {
                    return ApiResponse<string>.CreateFail("Email là bắt buộc.");
                }

                var account = await _unitOfWork.Accounts.GetByEmailAsync(request.Email);
                if (account == null)
                {
                    return ApiResponse<string>.CreateSuccess(string.Empty, "Kiểm tra hòm thư Email để lấy mã xác thực đặt lại mật khẩu.");
                }

                if (account.IsDeleted || !account.IsActive)
                {
                    return ApiResponse<string>.CreateSuccess(string.Empty, "Kiểm tra hòm thư Email để lấy mã xác thực đặt lại mật khẩu.");
                }

                var resetToken = GenerateResetPasswordCode();
                var cacheKey = $"auth:reset:{resetToken}";
                var payload = account.Id.ToString();

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                await _cache.SetStringAsync(cacheKey, payload, options, cancellationToken);

                try
                {
                    var emailBody = EmailTemplates.ForgotPasswordEmail(resetToken);
                    await _emailService.SendEmailAsync(request.Email, EmailTemplates.Subjects.ForgotPassword, emailBody, isHtml: true);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Lỗi khi gửi email reset password đến {Email}", request.Email);
                }

                return ApiResponse<string>.CreateSuccess(string.Empty, "Kiểm tra hòm thư Email để lấy mã xác thực đặt lại mật khẩu.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý forgot password");
                return ApiResponse<string>.CreateFail(ex);
            }
        }

        public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email) || 
                    string.IsNullOrWhiteSpace(request.ResetToken) || 
                    string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return ApiResponse<string>.CreateFail("Thiếu thông tin bắt buộc.");
                }

                var cacheKey = $"auth:reset:{request.ResetToken}";
                var accountIdStr = await _cache.GetStringAsync(cacheKey, cancellationToken);

                if (string.IsNullOrWhiteSpace(accountIdStr))
                {
                    return ApiResponse<string>.CreateFail("Mã xác nhận không hợp lệ hoặc đã hết hạn.", errorCode: 400);
                }

                if (!Guid.TryParse(accountIdStr, out var accountId))
                {
                    return ApiResponse<string>.CreateFail("Mã xác nhận không hợp lệ.", errorCode: 400);
                }

                var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
                if (account == null || account.Email != request.Email)
                {
                    return ApiResponse<string>.CreateFail("Thông tin không hợp lệ.", errorCode: 400);
                }

                if (account.IsDeleted)
                {
                    return ApiResponse<string>.CreateFail("Tài khoản đã bị vô hiệu.", errorCode: 403);
                }

                account.PasswordHash = _passwordHasher.HashPassword(account, request.NewPassword);
                account.IsPasswordChange = true;
                account.ModifiedDate = DateTime.UtcNow;

                _unitOfWork.Accounts.Update(account);
                await _unitOfWork.SaveChangesAsync();

                await _cache.RemoveAsync(cacheKey, cancellationToken);

                try
                {
                    var emailBody = EmailTemplates.PasswordResetConfirmationEmail(account.Email);
                    await _emailService.SendEmailAsync(account.Email, EmailTemplates.Subjects.PasswordResetConfirmation, emailBody, isHtml: true);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Lỗi khi gửi email xác nhận reset password đến {Email}", account.Email);
                }

                return ApiResponse<string>.CreateSuccess(string.Empty, "Đặt lại mật khẩu thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi reset password");
                return ApiResponse<string>.CreateFail(ex);
            }
        }

        public async Task<ApiResponse<string>> ChangePasswordAsync(Guid accountId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.OldPassword) || 
                    string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return ApiResponse<string>.CreateFail("Mật khẩu cũ và mật khẩu mới là bắt buộc.");
                }

                if (request.OldPassword == request.NewPassword)
                {
                    return ApiResponse<string>.CreateFail("Mật khẩu mới phải khác mật khẩu cũ.");
                }

                var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
                if (account == null)
                {
                    return ApiResponse<string>.CreateFail("Tài khoản không tồn tại.", errorCode: 404);
                }

                if (account.IsDeleted)
                {
                    return ApiResponse<string>.CreateFail("Tài khoản đã bị vô hiệu.", errorCode: 403);
                }

                if (!account.IsActive)
                {
                    return ApiResponse<string>.CreateFail("Tài khoản chưa được kích hoạt.", errorCode: 403);
                }

                var verifyResult = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, request.OldPassword);
                if (verifyResult == PasswordVerificationResult.Failed)
                {
                    return ApiResponse<string>.CreateFail("Mật khẩu cũ không đúng.", errorCode: 400);
                }

                account.PasswordHash = _passwordHasher.HashPassword(account, request.NewPassword);
                account.IsPasswordChange = true;
                account.ModifiedDate = DateTime.UtcNow;

                _unitOfWork.Accounts.Update(account);
                await _unitOfWork.SaveChangesAsync();

                try
                {
                    var emailBody = EmailTemplates.PasswordChangeConfirmationEmail(account.Email);
                    await _emailService.SendEmailAsync(account.Email, EmailTemplates.Subjects.PasswordChangeConfirmation, emailBody, isHtml: true);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Lỗi khi gửi email thông báo đổi password đến {Email}", account.Email);
                }

                return ApiResponse<string>.CreateSuccess(string.Empty, "Đổi mật khẩu thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đổi password");
                return ApiResponse<string>.CreateFail(ex);
            }
        }
    }
}
