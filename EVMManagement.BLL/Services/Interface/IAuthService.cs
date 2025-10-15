using System;
using System.Threading;
using System.Threading.Tasks;
using EVMManagement.BLL.DTOs.Request.Auth;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Auth;

namespace EVMManagement.BLL.Services.Interface
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginTokenDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> RegisterDealerAsync(RegisterDealerRequestDto request, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> CreateAccountAsync(CreateAccountRequestDto request, CancellationToken cancellationToken = default);
        Task<ApiResponse<LoginTokenDto>> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> ChangePasswordAsync(Guid accountId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default);
    }
}
