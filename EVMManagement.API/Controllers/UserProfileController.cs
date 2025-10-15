using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.BLL.DTOs.Request.User;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Models.Enums;
using EVMManagement.BLL.DTOs.Response.User;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _service;

        public UserProfileController(IUserProfileService service)
        {
            _service = service;
        }

       
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _service.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<UserProfileResponse>>.CreateSuccess(result));
        }

        [HttpGet("by-role")]
        public async Task<IActionResult> GetByRole([FromQuery] AccountRole role, [FromQuery] bool? isActive, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _service.GetByRoleAndStatusAsync(role, isActive, pageNumber, pageSize);
            
            return Ok(ApiResponse<PagedResult<UserProfileResponse>>.CreateSuccess(result));
        }

        [HttpGet("by-dealer/{dealerId}")]
        public async Task<IActionResult> GetByDealer(Guid dealerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<string>.CreateFail("PageNumber and PageSize must be greater than 0", null, 400));
            }

            var result = await _service.GetByDealerIdAsync(dealerId, pageNumber, pageSize);

            return Ok(ApiResponse<PagedResult<UserProfileResponse>>.CreateSuccess(result));
        }

        [HttpGet("by-account/{accId}")]
        public async Task<IActionResult> GetByAccountId(Guid accId)
        {
            var item = await _service.GetByAccountIdAsync(accId);
            if (item == null) return NotFound(ApiResponse<UserProfileResponse>.CreateFail("UserProfile not found", null, 404));
            return Ok(ApiResponse<UserProfileResponse>.CreateSuccess(item));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
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
            var existing = await _service.GetByAccountIdAsync(accId);
            if (existing == null) return NotFound(ApiResponse<UserProfileResponse>.CreateFail("UserProfile not found", null, 404));


            var toUpdate = new UserProfile
            {
                
                DealerId = dto.DealerId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                CardId = dto.CardId
            };

            var updated = await _service.UpdateAsync(accId, toUpdate);
            if (updated == null) return NotFound(ApiResponse<UserProfileResponse>.CreateFail("UserProfile not found", null, 404));
            return Ok(ApiResponse<UserProfileResponse>.CreateSuccess(updated));
        }

        

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateIsDeleted(Guid id, [FromQuery] bool isDeleted)
        {
            var updated = await _service.UpdateIsDeletedAsync(id, isDeleted);
            if (updated == null) return NotFound(ApiResponse<UserProfileResponse>.CreateFail("UserProfile not found", null, 404));
            return Ok(ApiResponse<UserProfileResponse>.CreateSuccess(updated));
        }

        
    }
}
