using Microsoft.AspNetCore.Mvc;
using EVMManagement.BLL.Services.Interface;
using EVMManagement.BLL.DTOs.Request.Promotion;
using EVMManagement.BLL.DTOs.Response;
using EVMManagement.BLL.DTOs.Response.Promotion;
using System.Threading.Tasks;
using System.Linq;

namespace EVMManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PromotionsController : ControllerBase
    {
        private readonly IPromotionService _service;

        public PromotionsController(IPromotionService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PromotionCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<PromotionResponseDto>.CreateFail("Validation failed", errors, 400));
            }

            var created = await _service.CreatePromotionAsync(dto);
            return CreatedAtAction(nameof(Create), new { id = created.Id }, ApiResponse<PromotionResponseDto>.CreateSuccess(created));
        }
    }
}

