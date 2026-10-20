using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using do_an_epu.Services.Interface;
using do_an_epu.DTOs;
using do_an_epu.DTOs.Common;
using Microsoft.AspNetCore.Authorization;

namespace do_an_epu.Controllers.API
{
    [Route("api/admin/room")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RoomController : ControllerBase
    {
        private readonly ILogger<RoomController> _logger;
        private readonly IRoomService _roomService;

        public RoomController(ILogger<RoomController> logger, IRoomService roomService)
        {
            _logger = logger;
            _roomService = roomService;
        }

        /// <summary>
        /// Lấy danh sách tất cả phòng của một khách sạn
        /// </summary>
        [HttpGet("list")]
        public async Task<ActionResult<List<RoomResponseDto>>> GetAll(int hotelId)
        {
            try
            {
                var rooms = await _roomService.GetAllAsync(hotelId);
                return Ok(ApiResponse<object>.Ok(rooms, "Success"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phòng");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách phòng theo loại phòng
        /// </summary>
        [HttpGet("by-roomtype/{roomTypeId}")]
        public async Task<ActionResult<List<RoomResponseDto>>> GetByRoomType(int roomTypeId)
        {
            try
            {
                var rooms = await _roomService.GetByRoomTypeAsync(roomTypeId);
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phòng theo loại");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách phòng theo tầng
        /// </summary>
        [HttpGet("by-floor/{floorId}")]
        public async Task<ActionResult<List<RoomResponseDto>>> GetByFloor(int floorId)
        {
            try
            {
                var rooms = await _roomService.GetRoomsByFloorAsync(floorId);
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phòng theo tầng");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách phòng trống
        /// </summary>
        [HttpGet("available")]
        public async Task<ActionResult<List<RoomResponseDto>>> GetAvailableRooms(int hotelId)
        {
            try
            {
                var rooms = await _roomService.GetAvailableRoomsAsync(hotelId);
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phòng trống");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết một phòng
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RoomResponseDto>> GetById(int id)
        {
            try
            {
                var room = await _roomService.GetByIdAsync(id);
                if (room == null)
                    return NotFound(new { error = "Phòng không tồn tại" });

                return Ok(room);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết phòng");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Tạo mới một phòng
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<RoomResponseDto>> Create(RoomCreateDto roomDto)
        {
            try
            {
                var exists = await _roomService.IsRoomNumberExistsAsync(roomDto.RoomNumber, roomDto.HotelId);
                if (exists)
                    return BadRequest(new { error = $"Số phòng {roomDto.RoomNumber} đã tồn tại" });

                var room = await _roomService.AddAsync(roomDto);
                return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo phòng");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, RoomUpdateDto roomDto)
        {
            Console.WriteLine($"Update Room ID {id}: {System.Text.Json.JsonSerializer.Serialize(roomDto)}");
            var room = await _roomService.UpdateAsync(id, roomDto);
                return Ok(ApiResponse<object>.Ok(room, "Success"));
            
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
                var result = await _roomService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { error = "Phòng không tồn tại" });

                return Ok(new { message = "Xóa phòng thành công" });
        }
    }
}
