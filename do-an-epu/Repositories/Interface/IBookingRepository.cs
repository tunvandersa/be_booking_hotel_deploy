using do_an_epu.Models;

namespace do_an_epu.Repositories.Interface
{
    public interface IBookingRepository
    {
        Task<Booking> CreateAsync(Booking booking);
        Task<Booking?> GetByIdAsync(int id);
        Task<Booking?> GetByBookingCodeAsync(string bookingCode);
        Task<List<Booking>> GetByUserIdAsync(int userId);
        Task<List<Booking>> GetAllAsync();
        Task<List<Booking>> SearchAsync(string keyword);
        Task<Booking> UpdateAsync(Booking booking);
        Task DeleteAsync(Booking booking);
        Task<List<Room>> GetAvailableRoomsAsync(int roomTypeId, DateTime checkIn, DateTime checkOut, int count);
    }
}
