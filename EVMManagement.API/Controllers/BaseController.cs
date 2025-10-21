using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EVMManagement.API.Services;
using EVMManagement.DAL.Models.Enums;

namespace EVMManagement.API.Controllers
{

    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected readonly IServiceFacade Services;

        protected BaseController(IServiceFacade services)
        {
            Services = services;
        }

        #region Helper Methods


        protected Guid? GetCurrentAccountId()
        {
            var accountIdClaim = User.FindFirst("Id")?.Value;
            if (string.IsNullOrWhiteSpace(accountIdClaim) || !Guid.TryParse(accountIdClaim, out var accountId))
            {
                return null;
            }
            return accountId;
        }


        protected AccountRole? GetCurrentRole()
        {
            var roleClaim = User.FindFirst("Role")?.Value;
            if (string.IsNullOrWhiteSpace(roleClaim) || !Enum.TryParse<AccountRole>(roleClaim, out var role))
            {
                return null;
            }
            return role;
        }


        protected string? GetCurrentEmail()
        {
            return User.FindFirst("Email")?.Value;
        }


        protected async Task<Guid?> GetCurrentUserDealerIdAsync()
        {
            var accountId = GetCurrentAccountId();
            if (!accountId.HasValue)
            {
                return null;
            }

            var userProfile = await Services.UserProfileService.GetByAccountIdAsync(accountId.Value);
            return userProfile?.DealerId;
        }


        protected bool IsEvmAdmin()
        {
            var role = GetCurrentRole();
            return role.HasValue && role.Value == AccountRole.EVM_ADMIN;
        }


        protected bool IsDealerManager()
        {
            var role = GetCurrentRole();
            return role.HasValue && role.Value == AccountRole.DEALER_MANAGER;
        }


        protected bool IsDealerStaff()
        {
            var role = GetCurrentRole();
            return role.HasValue && role.Value == AccountRole.DEALER_STAFF;
        }

        #endregion
    }
}

