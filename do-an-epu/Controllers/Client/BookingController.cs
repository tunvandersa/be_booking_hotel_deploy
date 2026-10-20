using Microsoft.AspNetCore.Mvc;
using do_an_epu.Services.Interface;
using do_an_epu.DTOs;
using do_an_epu.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using do_an_epu.Exceptions;

namespace do_an_epu.Controllers.Client
{
    [Route("api/client/booking")]
    [ApiController]
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

        /// <summary>
        /// Tạo booking mới
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateBooking([FromBody] BookingInfor booking)
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
        /// Lấy thông tin session hiện tại
        /// </summary>
        [HttpGet("session")]
        public async Task<IActionResult> GetBookingSession()
        {
            try
            {
                var sessionId = GetOrCreateSessionId();
                var result = await _bookingService.GetBookingSessionAsync(sessionId);
                return Ok(ApiResponse<BookingSession>.Ok(result, "Success"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail("Có lỗi xảy ra: " + ex.Message));
            }
        }

        /// <summary>
        /// Xóa toàn bộ session
        /// </summary>
        [HttpDelete("session")]
        public async Task<IActionResult> ClearBookingSession()
        {
            try
            {
                var sessionId = GetOrCreateSessionId();
                await _bookingService.ClearBookingSessionAsync(sessionId);
                return Ok(ApiResponse<object>.Ok(null, "Đã xóa session"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail("Có lỗi xảy ra: " + ex.Message));
            }
        }

        /// <summary>
        /// Checkout - hoàn tất đặt phòng
        /// </summary>
        [HttpPost("checkout")]
        [Authorize]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            try
            {
                var sessionId = GetOrCreateSessionId();
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.Fail("Unauthorized"));
                }

                // UserId là null vì chưa có authen, sau này lấy từ claims
                var result = await _bookingService.CheckoutAsync(sessionId, request, userId);
                return Ok(ApiResponse<object>.Ok(result, "Đặt phòng thành công"));
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

        [HttpGet("payment-callback")]
        public async Task<IActionResult> PaymentCallback()
        {
            try
            {
                var response = await _bookingService.ProcessPaymentCallback(Request.Query);

                if (response.Success)
                {
                    // Redirect to frontend success page
                    // Adjust URL based on your frontend routing
                    return Redirect($"http://localhost:5174/payment-success?bookingId={response.OrderId}");
                }
                
                return Redirect($"http://localhost:5174/payment-error");
            }
            catch (Exception ex)
            {
                // Log exception
                 return Redirect($"http://localhost:5174/payment-error");
            }
        }

        [HttpGet("my-bookings")]
        [Authorize]
        public async Task<IActionResult> GetMyBookings()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(ApiResponse<object>.Fail("Unauthorized"));
                }

                if (!int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest(ApiResponse<object>.Fail("Invalid User ID in token"));
                }

                var bookings = await _bookingService.GetBookingsByUserIdAsync(userId);
                return Ok(ApiResponse<List<BookingHistoryDto>>.Ok(bookings, "Success"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetBookingDetail(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.Fail("Unauthorized"));
                }

                var booking = await _bookingService.GetBookingDetailAsync(id, userId);
                return Ok(ApiResponse<BookingDetailDto>.Ok(booking, "Success"));
            }
            catch (NotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("cancel/{id}")]
        [Authorize]
        public async Task<IActionResult> CancelBooking(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.Fail("Unauthorized"));
                }

                await _bookingService.CancelBookingAsync(id, userId);
                return Ok(ApiResponse<object>.Ok(null, "Hủy phòng thành công"));
            }
            catch (NotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("{id}/details/{detailId}/upgrades")]
        [Authorize]
        public async Task<IActionResult> GetAvailableUpgrades(int id, int detailId)
        {
            try
            {
                var result = await _bookingService.GetAvailableUpgradesAsync(id, detailId);
                return Ok(ApiResponse<AvailableUpgradeDto>.Ok(result, "Success"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("{id}/upgrade")]
        [Authorize]
        public async Task<IActionResult> UpgradeRoom(int id, [FromBody] UpgradeRoomRequest request)
        {
            try
            {
                var paymentUrl = await _bookingService.UpgradeRoomAsync(id, request);
                return Ok(ApiResponse<string>.Ok(paymentUrl, "Vui lòng thanh toán để hoàn tất nâng cấp"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}
