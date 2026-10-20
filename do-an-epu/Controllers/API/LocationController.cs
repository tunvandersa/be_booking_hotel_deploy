using Microsoft.AspNetCore.Mvc;
using do_an_epu.Services.Interface;
using do_an_epu.DTOs.Common;
using do_an_epu.DTOs;
using do_an_epu.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace do_an_epu.Controllers.API
{
    [Route("api/admin/location")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class LocationController : ControllerBase
    {
        private readonly ILogger<LocationController> _logger;
        private readonly ILocationService _locationService;

        public LocationController(ILogger<LocationController> logger, ILocationService locationService)
        {
            _logger = logger;
            _locationService = locationService;
        }

        [HttpGet("list")]
        public async Task<ApiResponse<List<LocationResponseDto>>> GetAllLocations()
        {
            var result = await _locationService.GetAllLocationsAsync();
            return ApiResponse<List<LocationResponseDto>>.Ok(result, "Lấy danh sách địa điểm thành công");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLocationById(int id)
        {
            var result = await _locationService.GetLocationByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy địa điểm"));
            }
            return Ok(ApiResponse<LocationResponseDto>.Ok(result, "Lấy thông tin địa điểm thành công"));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateLocation([FromBody] LocationCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));
            }

            try
            {
                var result = await _locationService.CreateLocationAsync(model);
                return Ok(ApiResponse<LocationResponseDto>.Ok(result, "Thêm địa điểm thành công"));
            }
            catch (DomainException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateLocation(int id, [FromBody] LocationUpdateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));
            }

            try
            {
                var result = await _locationService.UpdateLocationAsync(id, model);
                return Ok(ApiResponse<LocationResponseDto>.Ok(result, "Cập nhật địa điểm thành công"));
            }
            catch (NotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(ex.Message));
            }
            catch (DomainException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var result = await _locationService.DeleteLocationAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy địa điểm"));
            }
            return Ok(ApiResponse<object>.Ok(null, "Xóa địa điểm thành công"));
        }
    }
}
