using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.User;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.BLL.DTOs.Response.User;
using EVMManagement.API.Services;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly IServiceFacade _services;

        public UserProfileController(IServiceFacade services)
        {
            _services = services;
        }

       
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.UserProfileService.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<UserProfileResponse>>.CreateSuccess(result));
        }

        [HttpGet("by-role")]
        public async Task<IActionResult> GetByRole([FromQuery] AccountRole role, [FromQuery] bool? isActive, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.UserProfileService.GetByRoleAndStatusAsync(role, isActive, pageNumber, pageSize);
            
            return Ok(ApiResponse<PagedResult<UserProfileResponse>>.CreateSuccess(result));
        }

        [HttpGet("by-dealer/{dealerId}")]
        public async Task<IActionResult> GetByDealer(Guid dealerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _services.UserProfileService.GetByDealerIdAsync(dealerId, pageNumber, pageSize);

            return Ok(ApiResponse<PagedResult<UserProfileResponse>>.CreateSuccess(result));
        }

        [HttpGet("by-account/{accId}")]
        public async Task<IActionResult> GetByAccountId(Guid accId)
        {
            var item = await _services.UserProfileService.GetByAccountIdAsync(accId);
            if (item == null) return NotFound(ApiResponse<UserProfileResponse>.CreateFail("Không tìm thấy hồ sơ người dùng.", null, 404));
            return Ok(ApiResponse<UserProfileResponse>.CreateSuccess(item));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.UserProfileService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<UserProfileResponse>.CreateFail("Không tìm thấy hồ sơ người dùng.", null, 404));
            return Ok(ApiResponse<UserProfileResponse>.CreateSuccess(item));
        }
       
        [HttpPatch("{accId}")]
        public Task<IActionResult> Update(Guid accId, [FromBody] UserProfilePatchDto dto)
            => UpdateInternalAsync(accId, dto);

        [HttpPut("{accId}")]
        public Task<IActionResult> Replace(Guid accId, [FromBody] UserProfileUpdateDto dto)
        {
            var patchDto = new UserProfilePatchDto
            {
                DealerId = dto.DealerId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                CardId = dto.CardId,
                Email = dto.Email
            };

            return UpdateInternalAsync(accId, patchDto);
        }

        private async Task<IActionResult> UpdateInternalAsync(Guid accId, UserProfilePatchDto dto)
        {
            if (dto == null)
            {
                return BadRequest(ApiResponse<UserProfileResponse>.CreateFail("Khong co du lieu duoc gui len.", null, 400));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<UserProfileResponse>.CreateFail("Dữ liệu không hợp lệ.", errors, 400));
            }

            var toUpdate = new UserProfile
            {
                DealerId = dto.DealerId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                CardId = dto.CardId
            };

            try
            {
                var updated = await _services.UserProfileService.UpdateAsync(accId, toUpdate, dto.Email);
                if (updated == null)
                {
                    return NotFound(ApiResponse<UserProfileResponse>.CreateFail("Không tìm thấy hồ sơ người dùng.", null, 404));
                }

                return Ok(ApiResponse<UserProfileResponse>.CreateSuccess(updated));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<UserProfileResponse>.CreateFail(ex.Message, null, 400));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<UserProfileResponse>.CreateFail(ex.Message, null, 400));
            }
            catch (DbUpdateException ex)
            {
                var constraintMessage = TryMapConstraintMessage(ex);
                if (constraintMessage != null)
                {
                    return BadRequest(ApiResponse<UserProfileResponse>.CreateFail(constraintMessage, null, 400));
                }

                var errors = ExtractExceptionMessages(ex, true);
                return BadRequest(ApiResponse<UserProfileResponse>.CreateFail("Không thể lưu hồ sơ người dùng do vi phạm ràng buộc dữ liệu.", errors, 400));
            }
            catch (Exception ex)
            {
                var errors = ExtractExceptionMessages(ex);
                return StatusCode(500, ApiResponse<UserProfileResponse>.CreateFail("Đã xảy ra lỗi khi cập nhật hồ sơ người dùng.", errors, 500));
            }
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await _services.UserProfileService.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<UserProfileResponse>.CreateFail("UserProfile not found", null, 404));
            return Ok(ApiResponse<UserProfileResponse>.CreateSuccess(updated));
        }

        private static readonly Dictionary<string, string> ConstraintErrorMessages = new(StringComparer.OrdinalIgnoreCase)
        {
            { "IX_UserProfiles_Phone", "Số điện thoại đã được sử dụng." },
            { "IX_UserProfiles_CardId", "Căn cước đã được sử dụng." },
            { "IX_Accounts_Email", "Email đã được sử dụng bởi tài khoản khác." }
        };

        private static List<string> ExtractExceptionMessages(Exception exception, bool skipGenericDbMessage = false)
        {
            var messages = new List<string>();
            var current = exception;
            var index = 0;

            while (current != null)
            {
                var message = (current.Message ?? string.Empty).Trim();

                if (!string.IsNullOrEmpty(message))
                {
                    if (skipGenericDbMessage && index == 0 && current is DbUpdateException)
                    {
                        messages.Add("Có lỗi cơ sở dữ liệu khi lưu thông tin người dùng.");
                    }
                    else
                    {
                        if (string.Equals(message, "An error occurred while saving changes to the database.", StringComparison.OrdinalIgnoreCase))
                        {
                            message = "Có lỗi cơ sở dữ liệu khi lưu thông tin người dùng.";
                        }

                        messages.Add(message);
                    }
                }

                current = current.InnerException;
                index++;
            }

            if (messages.Count == 0)
            {
                messages.Add("Lỗi không xác định.");
            }

            return messages;
        }

        private static string? TryMapConstraintMessage(DbUpdateException exception)
        {
            var current = exception as Exception;

            while (current != null)
            {
                var message = current.Message ?? string.Empty;
                foreach (var constraint in ConstraintErrorMessages)
                {
                    if (message.IndexOf(constraint.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                        return constraint.Value;
                }

                current = current.InnerException;
            }

            return null;
        }
    }
}
