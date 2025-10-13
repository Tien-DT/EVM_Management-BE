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
    }
}
