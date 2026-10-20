using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using do_an_epu.DTOs;
using do_an_epu.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using do_an_epu.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
namespace do_an_epu.Controllers.API
{
    [Route("api/admin/hotel")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class HotelController : ControllerBase
    {
        private readonly IHotelService _hotelService;

        public HotelController(IHotelService hotelService)
        {
            _hotelService = hotelService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateHotel([FromForm] HotelCreateDto hotel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    ApiResponse<object>.Fail("Dữ liệu không hợp lệ")
                );
            }

            var result = await _hotelService.AddAsync(hotel);

            return StatusCode(201,
                ApiResponse<object>.Ok(
                    result,
                    "Tạo khách sạn thành công"
                )
            );
        }
        [HttpGet("list")]
        public async Task<IActionResult> GetHotelList()
        {
            var result = await _hotelService.GetAllAsync();
            return Ok(ApiResponse<object>.Ok(result, "Success"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHotelById(int id)
        {
            var result = await _hotelService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail("Khách sạn không tìm thấy"));
            }
            return Ok(ApiResponse<object>.Ok(result, "Success"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
                var result = await _hotelService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(ApiResponse<object>.Fail("Khách sạn không tìm thấy"));
                }
                return Ok(ApiResponse<object>.Ok(null, "Xóa khách sạn thành công"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHotel(int id, [FromForm] HotelUpdateDto hotelDto)
        {
            var result = await _hotelService.UpdateAsync(id, hotelDto);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail("Khách sạn không tìm thấy"));
            }

            return Ok(ApiResponse<object>.Ok(result, "Cập nhật khách sạn thành công"));
        }
    }
}
