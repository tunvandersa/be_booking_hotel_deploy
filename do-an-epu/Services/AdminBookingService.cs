using do_an_epu.DTOs;
using do_an_epu.Models;
using do_an_epu.Repositories.Interface;
using do_an_epu.Services.Interface;
using do_an_epu.Exceptions;

namespace do_an_epu.Services
{
    public class AdminBookingService : IAdminBookingService
    {
        private readonly IBookingRepository _bookingRepository;

        public AdminBookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<IEnumerable<BookingAdminDto>> GetAllBookingsAsync(string? search = null)
        {
            List<Booking> bookings;
            if (!string.IsNullOrWhiteSpace(search))
            {
                bookings = await _bookingRepository.SearchAsync(search);
            }
            else
            {
                bookings = await _bookingRepository.GetAllAsync();
            }

            return bookings.Select(b => new BookingAdminDto
            {
                Id = b.Id,
                UserId = b.UserId,
                CustomerName = b.User != null ? $"{b.User.LastName} {b.User.FirstName}" : "Unknown",
                CustomerEmail = b.User?.Email ?? "",
                // Assuming all room types in a booking belong to same hotel, take first one
                HotelName = b.BookingDetails.FirstOrDefault()?.RoomType?.Hotel?.Name ?? "Unknown",
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                TotalPrice = b.TotalPrice,
                Status = b.Status.ToString(),
                NumberOfGuests = b.NumberOfGuests
            });
        }

        public async Task<bool> UpdateBookingStatusAsync(int id, string status)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) throw new NotFoundException("Booking not found");

            if (Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
            {
                booking.Status = bookingStatus;
                await _bookingRepository.UpdateAsync(booking);
                return true;
            }
            throw new Exception("Invalid status");
        }

        public async Task<bool> DeleteBookingAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) throw new NotFoundException("Booking not found");

            await _bookingRepository.DeleteAsync(booking);
            return true;
        }
    }
}
