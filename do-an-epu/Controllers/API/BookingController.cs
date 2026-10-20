using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using do_an_epu.Services.Interface;
using do_an_epu.DTOs;
using do_an_epu.DTOs.Common;
using Microsoft.AspNetCore.Authorization;

namespace do_an_epu.Controllers.API
{
    [Route("api/admin/booking")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // Lấy hoặc tạo session ID từ cookie
        private string GetOrCreateSessionId()
        {
            const string sessionCookieName = "BookingSessionId";
            
            if (Request.Cookies.TryGetValue(sessionCookieName, out var sessionId) && !string.IsNullOrEmpty(sessionId))
            {
                return sessionId;
            }

            // Tạo session ID mới
            sessionId = Guid.NewGuid().ToString();
            
            // Lưu vào cookie
            Response.Cookies.Append(sessionCookieName, sessionId, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(30)
            });

            return sessionId;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBooking(BookingInfor booking)
        {
            var result = await _bookingService.CreateBookingAsync(booking);
            return Ok(ApiResponse<object>.Ok(result, "Success"));
        }

        /// <summary>
        /// Thêm phòng vào session booking
        /// </summary>
        [HttpPost("session/add-room")]
        public async Task<IActionResult> AddRoomToSession([FromBody] AddRoomRequest request)
        {
            try
            {
                var sessionId = GetOrCreateSessionId();
                var result = await _bookingService.AddRoomToSessionAsync(sessionId, request);
                return Ok(ApiResponse<BookingSession>.Ok(result, "Đã thêm phòng vào danh sách"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail("Có lỗi xảy ra: " + ex.Message));
            }
        }

        /// <summary>
        /// Xóa phòng khỏi session booking
        /// </summary>
        [HttpPost("session/remove-room")]
        public async Task<IActionResult> RemoveRoomFromSession([FromBody] RemoveRoomRequest request)
        {
            try
            {
                var sessionId = GetOrCreateSessionId();
                var result = await _bookingService.RemoveRoomFromSessionAsync(sessionId, request);
                return Ok(ApiResponse<BookingSession>.Ok(result, "Đã xóa phòng khỏi danh sách"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail("Có lỗi xảy ra: " + ex.Message));
            }
        }

        /// <summary>
        /// Lấy thông tin session booking hiện tại
        /// </summary>
        [HttpGet("session")]
        public async Task<IActionResult> GetBookingSession()
        {
            try
            {
                var sessionId = GetOrCreateSessionId();
                var result = await _bookingService.GetBookingSessionAsync(sessionId);
                
                if (result == null)
                {
                    return Ok(ApiResponse<BookingSession>.Ok(new BookingSession { Rooms = new List<RoomBookingItem>() }, "Session rỗng"));
                }
                
                return Ok(ApiResponse<BookingSession>.Ok(result, "Lấy thông tin session thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail("Có lỗi xảy ra: " + ex.Message));
            }
        }

        /// <summary>
        /// Xóa toàn bộ session booking
        /// </summary>
        [HttpDelete("session")]
        public async Task<IActionResult> ClearBookingSession()
        {
            try
            {
                var sessionId = GetOrCreateSessionId();
                await _bookingService.ClearBookingSessionAsync(sessionId);
                return Ok(ApiResponse<object>.Ok(null, "Đã xóa session booking"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail("Có lỗi xảy ra: " + ex.Message));
            }
        }

        /// <summary>
        /// Checkout - Hoàn tất đặt phòng
        /// </summary>
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            try
            {
                var sessionId = GetOrCreateSessionId();
                
                // Lấy userId nếu đã đăng nhập (có thể từ JWT token)
                int? userId = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                    {
                        userId = parsedUserId;
                    }
                }

                var result = await _bookingService.CheckoutAsync(sessionId, request, userId);
                return Ok(ApiResponse<CheckoutResponse>.Ok(result, "Đặt phòng thành công!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail("Có lỗi xảy ra: " + ex.Message));
            }
        }
    }
}
