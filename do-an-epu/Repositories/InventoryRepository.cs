using do_an_epu.Repositories.Interface;
using do_an_epu.Data;
using do_an_epu.Models;
using Microsoft.EntityFrameworkCore;
namespace do_an_epu.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly ApplicationDbContext _context;

        public InventoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<RoomInventory>> GetByDateRangeAndRoomTypeIdsAsync(DateOnly startDate,DateOnly endDate,List<int> roomTypeIds)
        {
            return await _context.RoomInventory
                .Where(i => i.Date >= startDate
                         && i.Date < endDate
                         && roomTypeIds.Contains(i.RoomTypeId))
                .ToListAsync();
        }
        public async Task<List<RoomInventory>> GetAllByRoomTypeIdAsync(int id)
        {
            return await _context.RoomInventory
                            .Where(rt => rt.Id == id)
                            .ToListAsync();
        }
        public async Task<List<RoomInventory>> GetByRoomTypeIdFromDateAsync(int roomTypeId, DateOnly startDate)
        {
            return await _context.RoomInventory
                            .Where(ri => ri.RoomTypeId == roomTypeId && ri.Date >= startDate)
                            .ToListAsync();
        }

        /// <summary>
        /// Lấy số phòng trống tối thiểu trong khoảng ngày (để biết có thể đặt tối đa bao nhiêu phòng)
        /// </summary>
        public async Task<int> GetMinAvailableRoomsAsync(int roomTypeId, DateOnly startDate, DateOnly endDate)
        {
            var inventories = await _context.RoomInventory
                .Where(i => i.RoomTypeId == roomTypeId
                         && i.Date >= startDate
                         && i.Date < endDate)
                .ToListAsync();

            if (!inventories.Any())
                return 0;

            // Trả về số phòng trống ít nhất trong khoảng thời gian
            return inventories.Min(i => i.AvailableRooms);
        }

        /// <summary>
        /// Cập nhật số phòng đã đặt (tăng BookedRooms, giảm AvailableRooms)
        /// </summary>
        public async Task UpdateBookedRoomsAsync(int roomTypeId, DateOnly startDate, DateOnly endDate, int roomCount)
        {
            var inventories = await _context.RoomInventory
                .Where(i => i.RoomTypeId == roomTypeId
                         && i.Date >= startDate
                         && i.Date < endDate)
                .ToListAsync();

            foreach (var inventory in inventories)
            {
                inventory.BookedRooms += roomCount;
                inventory.AvailableRooms = Math.Max(0, inventory.TotalRooms - inventory.BookedRooms);
            }

            await _context.SaveChangesAsync();
        }
    }
}
