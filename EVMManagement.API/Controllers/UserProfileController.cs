using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
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
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<UserProfileResponse>>.CreateSuccess(list));
        }

        [HttpGet("by-role")]
        public async Task<IActionResult> GetByRole([FromQuery] AccountRole role, [FromQuery] bool? isActive)
        {
            var list = await _service.GetByRoleAndStatusAsync(role, isActive);
            return Ok(ApiResponse<IEnumerable<UserProfileResponse>>.CreateSuccess(list));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<UserProfileResponse>.CreateFail("UserProfile not found", null, 404));
            return Ok(ApiResponse<UserProfileResponse>.CreateSuccess(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserProfileCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<UserProfileResponse>.CreateFail("Validation failed", errors, 400));
            }

            var toCreate = new UserProfile
            {
                AccountId = dto.AccountId,
                DealerId = dto.DealerId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                CardId = dto.CardId
            };

            var created = await _service.CreateAsync(toCreate);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<UserProfileResponse>.CreateSuccess(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserProfileUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<UserProfileResponse>.CreateFail("Validation failed", errors, 400));
            }
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound(ApiResponse<UserProfileResponse>.CreateFail("UserProfile not found", null, 404));


            var toUpdate = new UserProfile
            {
                AccountId = dto.AccountId,
                DealerId = dto.DealerId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                CardId = dto.CardId
            };

            var updated = await _service.UpdateAsync(id, toUpdate);
            if (updated == null) return NotFound(ApiResponse<UserProfileResponse>.CreateFail("UserProfile not found", null, 404));
            return Ok(ApiResponse<UserProfileResponse>.CreateSuccess(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound(ApiResponse<string>.CreateFail("UserProfile not found", null, 404));
            return Ok(ApiResponse<string>.CreateSuccess("Deleted"));
        }

        
    }
}
