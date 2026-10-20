using do_an_epu.DTOs;
using do_an_epu.DTOs.VnPay;

namespace do_an_epu.Services.Interface
{
    public interface IBookingService
    {
        Task<BookingDto> CreateBookingAsync(BookingInfor booking);
        Task<BookingSession> AddRoomToSessionAsync(string sessionId, AddRoomRequest request);
        Task<BookingSession> RemoveRoomFromSessionAsync(string sessionId, RemoveRoomRequest request);
        Task<BookingSession> GetBookingSessionAsync(string sessionId);
        Task ClearBookingSessionAsync(string sessionId);
        Task<CheckoutResponse> CheckoutAsync(string sessionId, CheckoutRequest request, int? userId);
        Task<PaymentResponseModel> ProcessPaymentCallback(IQueryCollection collections);
        Task<List<BookingHistoryDto>> GetBookingsByUserIdAsync(int userId);
        Task<bool> CancelBookingAsync(int bookingId, int userId);
        Task<BookingDetailDto> GetBookingDetailAsync(int bookingId, int userId);
        
        // Upgrade features
        Task<AvailableUpgradeDto> GetAvailableUpgradesAsync(int bookingId, int bookingDetailId);
        Task<string> UpgradeRoomAsync(int bookingId, UpgradeRoomRequest request);
    }
}
