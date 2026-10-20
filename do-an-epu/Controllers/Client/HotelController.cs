using Microsoft.AspNetCore.Mvc;
using do_an_epu.Services.Interface;
using do_an_epu.DTOs;
using do_an_epu.DTOs.Common;

namespace do_an_epu.Controllers.Client
{
    [Route("api/client/hotel")]
    [ApiController]
    public class HotelController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        private readonly IRoomTypeService _roomTypeService;

        public HotelController(IHotelService hotelService, IRoomTypeService roomTypeService)
        {
            _hotelService = hotelService;
            _roomTypeService = roomTypeService;
        }

        /// <summary>
        /// Lấy danh sách khách sạn (chỉ Active)
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetHotelList()
        {
            var result = await _hotelService.GetActiveHotelsAsync();
            return Ok(ApiResponse<object>.Ok(result, "Success"));
        }

        /// <summary>
        /// Lấy chi tiết khách sạn theo ID
        /// </summary>
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

        /// <summary>
        /// Lấy danh sách loại phòng theo khách sạn
        /// </summary>
        [HttpGet("{hotelId}/room-types")]
        public async Task<IActionResult> GetRoomTypesByHotel(int hotelId)
        {
            var result = await _roomTypeService.GetRoomTypesByHotelAsync(hotelId);
            return Ok(ApiResponse<object>.Ok(result, "Success"));
        }
    }
}
