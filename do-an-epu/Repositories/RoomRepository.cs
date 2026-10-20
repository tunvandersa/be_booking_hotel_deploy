using do_an_epu.Models;
using do_an_epu.Repositories.Interface;
using do_an_epu.Data;
using Microsoft.EntityFrameworkCore;

namespace do_an_epu.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly ApplicationDbContext _context;

        public RoomRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Room> AddAsync(Room room)
        {
            await _context.AddAsync(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task<List<Room>> GetAllAsync(int hotelId)
        {
            return await _context.Rooms
                .Where(r => !r.IsDeleted && r.Floor.HotelId == hotelId)
                .Include(r => r.Floor)
                .Include(r => r.RoomType)
                    .ThenInclude(rt => rt.RoomTypeCategory)
                .ToListAsync();
        }

        public async Task<List<Room>> GetByRoomTypeAsync(int roomTypeId)
        {
            return await _context.Rooms
                .Where(r => !r.IsDeleted && r.RoomTypeId == roomTypeId)
                .Include(r => r.Floor)
                .Include(r => r.RoomType)
                   .ThenInclude(rt => rt.RoomTypeCategory)
                .ToListAsync();
        }

        public async Task<int> CountActiveRoomsByRoomTypeAsync(int roomTypeId)
        {
            return await _context.Rooms
                .Where(r => r.RoomTypeId == roomTypeId
                         && !r.IsDeleted
                         && r.Status != RoomStatus.Maintenance)
                .CountAsync();
        }

        public async Task<Room> GetByIdAsync(int id)
        {
            return await _context.Rooms
                .Include(r => r.Floor)
                .Include(r => r.RoomType)
                .Include(r => r.BookingDetails)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        public async Task<Room> UpdateAsync(Room room)
        {
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
            var updatedRoom = await _context.Rooms
                .Include(r => r.RoomType).
                    ThenInclude(rt => rt.RoomTypeCategory)
                .FirstOrDefaultAsync(r => r.Id == room.Id);
            return updatedRoom;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var room = await GetByIdAsync(id);
            if (room == null)
                return false;

            room.IsDeleted = true;
            room.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(room);
            return true;
        }

        public async Task<bool> IsRoomNumberExistsAsync(int roomNumber, int hotelId)
        {
            return await _context.Rooms
                .Where(r => !r.IsDeleted && r.RoomNumber == roomNumber && r.Floor.HotelId == hotelId)
                .AnyAsync();
        }

        public async Task<List<Room>> GetRoomsByFloorAsync(int floorId)
        {
            return await _context.Rooms
                .Where(r => !r.IsDeleted && r.FloorId == floorId)
                .Include(r => r.RoomType)
                .ToListAsync();
        }

        public async Task<List<Room>> GetAvailableRoomsAsync(int hotelId)
        {
            return await _context.Rooms
                .Where(r => !r.IsDeleted && r.IsActive && r.Status == RoomStatus.Available && r.Floor.HotelId == hotelId)
                .Include(r => r.Floor)
                .Include(r => r.RoomType)
                .ToListAsync();
        }
    }
}
