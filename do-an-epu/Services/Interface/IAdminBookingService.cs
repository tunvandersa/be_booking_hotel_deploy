using do_an_epu.DTOs;
using do_an_epu.Models;

namespace do_an_epu.Services.Interface
{
    public interface IAdminBookingService
    {
        Task<IEnumerable<BookingAdminDto>> GetAllBookingsAsync(string? search = null);
        Task<bool> UpdateBookingStatusAsync(int id, string status);
        Task<bool> DeleteBookingAsync(int id);
    }
}
