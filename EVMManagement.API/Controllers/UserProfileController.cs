using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.DTOs.Request.User;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.BLL.DTOs.Response.User;
using EVMManagement.API.Services;

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
            if (item == null) return NotFound(ApiResponse<UserProfileResponse>.CreateFail("UserProfile not found", null, 404));
            return Ok(ApiResponse<UserProfileResponse>.CreateSuccess(item));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _services.UserProfileService.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<UserProfileResponse>.CreateFail("UserProfile not found", null, 404));
            return Ok(ApiResponse<UserProfileResponse>.CreateSuccess(item));
        }
       
        [HttpPut("{accId}")]
        public async Task<IActionResult> Update(Guid accId, [FromBody] UserProfileUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<UserProfileResponse>.CreateFail("Validation failed", errors, 400));
            }
            var existing = await _services.UserProfileService.GetByAccountIdAsync(accId);
            if (existing == null) return NotFound(ApiResponse<UserProfileResponse>.CreateFail("UserProfile not found", null, 404));


            var toUpdate = new UserProfile
            {
                
                DealerId = dto.DealerId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                CardId = dto.CardId
            };

            var updated = await _services.UserProfileService.UpdateAsync(accId, toUpdate);
            if (updated == null) return NotFound(ApiResponse<UserProfileResponse>.CreateFail("UserProfile not found", null, 404));
            return Ok(ApiResponse<UserProfileResponse>.CreateSuccess(updated));
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await _services.UserProfileService.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<UserProfileResponse>.CreateFail("UserProfile not found", null, 404));
            return Ok(ApiResponse<UserProfileResponse>.CreateSuccess(updated));
        }

        
    }
}
