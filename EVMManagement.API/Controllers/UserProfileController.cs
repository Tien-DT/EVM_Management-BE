using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.API.DTOs.Request;
using EVMManagement.API.DTOs.Response;
using EVMManagement.DAL.Models.Entities;

namespace EVMManagement.API.Controllers
{
    [Route("api/[controller]")]
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
            return Ok(ApiResponse<IEnumerable<UserProfile>>.CreateSuccess(list));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<UserProfile>.CreateFail("UserProfile not found", null, 404));
            return Ok(ApiResponse<UserProfile>.CreateSuccess(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserProfileCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<UserProfile>.CreateFail("Validation failed", errors, 400));
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
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<UserProfile>.CreateSuccess(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserProfileUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<UserProfile>.CreateFail("Validation failed", errors, 400));
            }
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound(ApiResponse<UserProfile>.CreateFail("UserProfile not found", null, 404));


            var toUpdate = new UserProfile
            {
                // Do not set AccountId here to avoid changing it in service
                DealerId = dto.DealerId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                CardId = dto.CardId
            };

            var updated = await _service.UpdateAsync(id, toUpdate);
            if (updated == null) return NotFound(ApiResponse<UserProfile>.CreateFail("UserProfile not found", null, 404));
            return Ok(ApiResponse<UserProfile>.CreateSuccess(updated));
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
