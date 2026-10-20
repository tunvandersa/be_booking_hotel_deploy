using Azure.Core;
using do_an_epu.Data;
using do_an_epu.Models;
using do_an_epu.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace do_an_epu.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Booking> CreateAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.BookingDetails)
                    .ThenInclude(bd => bd.RoomType)
                        .ThenInclude(rt => rt.RoomTypeCategory)
                .Include(b => b.BookingDetails)
                    .ThenInclude(bd => bd.RoomType)
                        .ThenInclude(rt => rt.Hotel)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Booking?> GetByBookingCodeAsync(string bookingCode)
        {
            // Tìm booking theo mã booking (có thể lưu trong một field riêng hoặc dùng Id)
            if (int.TryParse(bookingCode.Replace("BK", ""), out int id))
            {
                return await GetByIdAsync(id);
            }
            return null;
        }

        public async Task<List<Booking>> GetByUserIdAsync(int userId)
        {
            return await _context.Bookings
                .Include(b => b.BookingDetails)
                    .ThenInclude(bd => bd.RoomType)
                        .ThenInclude(rt => rt.Hotel)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.BookingDetails)
                    .ThenInclude(bd => bd.RoomType)
                        .ThenInclude(rt => rt.RoomTypeCategory)
                .Include(b => b.BookingDetails)
                    .ThenInclude(bd => bd.RoomType)
                        .ThenInclude(rt => rt.Hotel)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<List<Booking>> SearchAsync(string keyword)
        {
             return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.BookingDetails)
                    .ThenInclude(bd => bd.RoomType)
                        .ThenInclude(rt => rt.RoomTypeCategory)
                .Include(b => b.BookingDetails)
                    .ThenInclude(bd => bd.RoomType)
                        .ThenInclude(rt => rt.Hotel)
                .Where(b => b.Id.ToString().Contains(keyword) ||
                            (b.User.FirstName + " " + b.User.LastName).Contains(keyword) ||
                            b.User.Email.Contains(keyword) ||
                            b.BookingDetails.Any(bd => bd.RoomType.Hotel.Name.Contains(keyword)))
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<Booking> UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task DeleteAsync(Booking booking)
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Lấy danh sách phòng trống theo loại phòng trong khoảng thời gian
        /// </summary>
        public async Task<List<Room>> GetAvailableRoomsAsync(int roomTypeId, DateTime checkIn, DateTime checkOut, int count)
        {
            //Lấy danh sách roomId đã được đặt trong khoảng thời gian này
            var bookedRoomIds = await _context.BookingDetails
                .Include(bd => bd.Booking)
                .Where(bd => bd.Room.RoomTypeId == roomTypeId
                    && bd.Booking.Status != BookingStatus.Cancelled
                    && bd.Booking.CheckInDate < checkOut
                    && bd.Booking.CheckOutDate > checkIn)
                .Select(bd => bd.RoomId)
                .Distinct()
                .ToListAsync();

            var checkInDate = DateOnly.FromDateTime(checkIn);
            var checkOutDate = DateOnly.FromDateTime(checkOut);
            // Lấy danh sách roomId bị block trong khoảng thời gian này
            var blockedRoomIds = await _context.RoomBlocks
                .Where(rb => rb.Room.RoomTypeId == roomTypeId
                    && rb.StartDate < checkOutDate
                    && rb.EndDate > checkInDate)
                .Select(rb => rb.RoomId)
                .Distinct()
                .ToListAsync();

            // Lấy các phòng available
            var availableRooms = await _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.Floor)
                .Where(r => r.RoomTypeId == roomTypeId
                    && r.IsActive
                    && !r.IsDeleted
                    && r.Status == RoomStatus.Available
                    && !bookedRoomIds.Contains(r.Id)
                    && !blockedRoomIds.Contains(r.Id))
                .Take(count)
                .ToListAsync();

            return availableRooms;
        }
    }
}
