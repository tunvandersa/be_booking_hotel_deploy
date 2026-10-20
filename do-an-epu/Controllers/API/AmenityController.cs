using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using do_an_epu.Services.Interface;
using do_an_epu.DTOs.Common;
using do_an_epu.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace do_an_epu.Controllers.API
{
    [Route("api/admin/amenity")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AmenityController : ControllerBase
    {
        private readonly ILogger<AmenityController> _logger;
        private readonly IAmenityService _amenityService;
        public AmenityController(ILogger<AmenityController> logger, IAmenityService amenityService)
        {
            _logger = logger;
            _amenityService = amenityService;
        }
        [HttpGet("list_category")]
        public async Task<ApiResponse<List<AmenityCategoryDto>>> GetAmenities()
        {
            var result = await _amenityService.GetAllAmenitiesCategoryAsync();

            return ApiResponse<List<AmenityCategoryDto>>.Ok(result, "Success");
        }
        [HttpPost("create_category")]
        public async Task<ApiResponse<AmenityCategoryDto>> CreateAmenityCategory([FromBody] AmenityCategoryCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                return ApiResponse<AmenityCategoryDto>.Fail("Invalid input data.");
            }

            var createdAmenityCategory = await _amenityService.AddAmenityCategoryAsync(model);

            var resultDto = new AmenityCategoryDto
            {
                Id = createdAmenityCategory.Id,
                Name = createdAmenityCategory.Name
            };

            return ApiResponse<AmenityCategoryDto>.Ok(resultDto, "Thêm danh mục tiện nghi thành công");
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateAmenity([FromForm] AmenityCreateDto model)
        {
            var result = await _amenityService.AddAmenityAsync(model);

            return Ok(ApiResponse<object>.Ok(result, "Thêm tiện nghi thành công"));
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetAmenityById(int id)
        {

            var result = await _amenityService.GetAmenityByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail("Amenity not found."));
            }

            var responseDto = new AmenityResponseDto
            {
                Id = result.Id,
                Name = result.Name,
                IconUrl = result.IconUrl,
                IsActive = result.IsActive
            };

            return Ok(ApiResponse<AmenityResponseDto>.Ok(responseDto, "Success"));
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateAmenity(int id, [FromForm] AmenityCreateDto model, IFormFile iconFile)
        {
            var existingAmenity = await _amenityService.GetAmenityByIdAsync(id);
            if (existingAmenity == null)
            {
                return NotFound(ApiResponse<object>.Fail("Amenity not found."));
            }
            var result = await _amenityService.UpdateAmenityAsync(id, model);

            return Ok(ApiResponse<object>.Ok(result, "Amenity updated successfully."));

        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteAmenity(int id)
        {
            var result = await _amenityService.DeleteAmenityAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.Fail("Amenity not found."));
            }

            return Ok(ApiResponse<object>.Ok(null, "Amenity deleted successfully."));
        }
    }
}
