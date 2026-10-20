using Microsoft.AspNetCore.Mvc;
using do_an_epu.Services.Interface;
using do_an_epu.DTOs.Common;
using do_an_epu.DTOs;
using do_an_epu.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace do_an_epu.Controllers.API
{
    [Route("api/admin/floor")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class FloorController : ControllerBase
    {
        private readonly ILogger<FloorController> _logger;
        private readonly IFloorService _floorService;

        public FloorController(ILogger<FloorController> logger, IFloorService floorService)
        {
            _logger = logger;
            _floorService = floorService;
        }

        [HttpGet("list")]
        public async Task<ApiResponse<List<FloorResponseDto>>> GetAllFloors()
        {
            var result = await _floorService.GetAllFloorsAsync();
            return ApiResponse<List<FloorResponseDto>>.Ok(result, "Lấy danh sách tầng thành công");
        }

        [HttpGet("hotel/{hotelId}")]
        public async Task<ApiResponse<List<FloorResponseDto>>> GetFloorsByHotelId(int hotelId)
        {
            var result = await _floorService.GetFloorsByHotelIdAsync(hotelId);
            return ApiResponse<List<FloorResponseDto>>.Ok(result, "Lấy danh sách tầng theo khách sạn thành công");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFloorById(int id)
        {
            var result = await _floorService.GetFloorByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy tầng"));
            }
            return Ok(ApiResponse<FloorResponseDto>.Ok(result, "Lấy thông tin tầng thành công"));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateFloor([FromBody] FloorCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));
            }

            try
            {
                var result = await _floorService.CreateFloorAsync(model);
                return Ok(ApiResponse<FloorResponseDto>.Ok(result, "Thêm tầng thành công"));
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

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateFloor(int id, [FromBody] FloorUpdateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));
            }

            try
            {
                var result = await _floorService.UpdateFloorAsync(id, model);
                return Ok(ApiResponse<FloorResponseDto>.Ok(result, "Cập nhật tầng thành công"));
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
        public async Task<IActionResult> DeleteFloor(int id)
        {
            var result = await _floorService.DeleteFloorAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy tầng"));
            }
            return Ok(ApiResponse<object>.Ok(null, "Xóa tầng thành công"));
        }
    }
}
