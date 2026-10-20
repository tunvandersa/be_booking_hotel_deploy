using Microsoft.AspNetCore.Mvc;
using do_an_epu.DTOs;
using do_an_epu.Services.Interface;
using do_an_epu.DTOs.Common;
using Microsoft.AspNetCore.Authorization;

namespace do_an_epu.Controllers.API
{
    [Route("api/admin/roomtype")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RoomTypeController : ControllerBase
    {
        private readonly IRoomTypeService _roomTypeService;

        public RoomTypeController(IRoomTypeService roomTypeService)
        {
            _roomTypeService = roomTypeService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRoomType([FromForm] RoomTypeCreateDto roomTypeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    ApiResponse<object>.Fail("Dữ liệu không hợp lệ")
                );
            }

            var result = await _roomTypeService.AddAsync(roomTypeDto);

            return StatusCode(201,
                ApiResponse<object>.Ok(
                    result,
                    "Tạo loại phòng thành công"
                )
            );
        }

        [HttpGet("categories/list")]
        public async Task<IActionResult> GetRoomTypeCategorys()
        {
            var result = await _roomTypeService.GetAllCategoriesAsync();
            return Ok(ApiResponse<object>.Ok(result, "Lấy danh sách loại phòng thành công"));
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetRoomTypeList([FromQuery] int hotelId)
        {
            var result = await _roomTypeService.GetAllAsync(hotelId);
            return Ok(ApiResponse<object>.Ok(result, "Lấy danh sách loại phòng thành công"));
        }

        [HttpGet("hotel/{hotelId}")]
        public async Task<IActionResult> GetRoomTypeByHotel(int hotelId)
        {
            var result = await _roomTypeService.GetRoomTypesByHotelAsync(hotelId);
            return Ok(ApiResponse<object>.Ok(result, "Lấy loại phòng theo khách sạn thành công"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomTypeById(int id)
        {
            var result = await _roomTypeService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail("Loại phòng không tồn tại"));
            }
            return Ok(ApiResponse<object>.Ok(result, "Lấy thông tin loại phòng thành công"));
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateRoomType(int id, [FromForm] RoomTypeUpdateDto roomTypeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));
            }

            var result = await _roomTypeService.UpdateAsync(id, roomTypeDto);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail("Loại phòng không tồn tại"));
            }

            return Ok(ApiResponse<object>.Ok(result, "Cập nhật loại phòng thành công"));
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteRoomType(int id)
        {
            var result = await _roomTypeService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.Fail("Loại phòng không tồn tại"));
            }

            return Ok(ApiResponse<object>.Ok(null, "Xóa loại phòng thành công"));
        }

        // RoomTypeCategory endpoints
        [HttpPost("categories/create")]
        public async Task<IActionResult> CreateCategory([FromBody] RoomTypeCategoryCreateDto categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));
            }

            try
            {
                var result = await _roomTypeService.AddCategoryAsync(categoryDto);
                return Ok(ApiResponse<RoomTypeCategoryResponseDto>.Ok(result, "Thêm danh mục loại phòng thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("categories/{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var result = await _roomTypeService.GetCategoryByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy danh mục"));
            }
            return Ok(ApiResponse<RoomTypeCategoryResponseDto>.Ok(result, "Lấy thông tin danh mục thành công"));
        }

        [HttpPut("categories/update/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] RoomTypeCategoryUpdateDto categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ"));
            }

            try
            {
                var result = await _roomTypeService.UpdateCategoryAsync(id, categoryDto);
                return Ok(ApiResponse<RoomTypeCategoryResponseDto>.Ok(result, "Cập nhật danh mục thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpDelete("categories/delete/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _roomTypeService.DeleteCategoryAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.Fail("Không tìm thấy danh mục"));
            }
            return Ok(ApiResponse<object>.Ok(null, "Xóa danh mục thành công"));
        }
    }
}
