using do_an_epu.Repositories.Interface;
using do_an_epu.Services.Interface;
using do_an_epu.Exceptions;
using AutoMapper;
using do_an_epu.Models;
using Microsoft.EntityFrameworkCore;
using do_an_epu.Data;
using do_an_epu.DTOs;

namespace do_an_epu.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IRoomTypeRepository _roomTypeRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        public InventoryService(ApplicationDbContext context, IInventoryRepository inventoryRepository, IRoomTypeRepository roomTypeRepository, IMapper mapper)
        {
            _inventoryRepository = inventoryRepository;
            _mapper = mapper;
            _roomTypeRepository = roomTypeRepository;
            _context = context;
        }
        public async Task<bool> CreateInventoryAsync(DateOnly startDate, DateOnly endDate, int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var roomTypes = await _roomTypeRepository.GetAllAsync(id);
                if (!roomTypes.Any())
                {
                    throw new NotFoundException("Khách sạn chưa có loại phòng");
                }

                var roomTypeIds = roomTypes.Select(r => r.Id).ToList();

                var existingInventories = await _inventoryRepository
                    .GetByDateRangeAndRoomTypeIdsAsync(startDate, endDate, roomTypeIds);

                var inventorySet = new HashSet<(DateOnly Date, int RoomTypeId)>(
                    existingInventories.Select(i => (i.Date, i.RoomTypeId))
                );

                var newInventories = new List<RoomInventory>();

                foreach (var roomType in roomTypes)
                {
                    var totalRooms = roomType.Rooms
                                        .Count(r => !r.IsDeleted && r.Status != RoomStatus.Maintenance);
                    for (var date = startDate; date < endDate; date = date.AddDays(1))
                    {
                        if (!inventorySet.Contains((date, roomType.Id)))
                        {
                            newInventories.Add(new RoomInventory
                            {
                                Date = date,
                                RoomTypeId = roomType.Id,
                                TotalRooms = totalRooms,
                                BookedRooms = 0,
                                AvailableRooms = totalRooms
                            });
                        }
                    }
                }

                if (newInventories.Any())
                {
                    await _context.AddRangeAsync(newInventories);
                }
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateInventoryAsync(int id, UpdateInventoryDto dto)
        {
            var inventory = await _context.RoomInventory.FindAsync(id);
            if (inventory == null)
            {
                throw new NotFoundException("Inventory record not found");
            }

            inventory.TotalRooms = dto.TotalRooms;
            inventory.AvailableRooms = dto.AvailableRooms;
            // You might want to update BookedRooms or BlockRooms logic here if needed, 
            // but for simple edit we trust the input or validate it.
            // For example: AvailableRooms <= TotalRooms

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpInventoryAsync(int roomTypeId, DateOnly startDate) // Thêm tham số effectiveDate
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Lấy RoomType và KÈM THEO danh sách phòng (Quan trọng: Phải load Rooms)
                // Giả sử repository của bạn hỗ trợ include, hoặc dùng trực tiếp DbContext để chắc chắn
                var roomType = await _context.RoomTypes
                    .Include(rt => rt.Rooms)
                    .FirstOrDefaultAsync(rt => rt.Id == roomTypeId)
                    ?? throw new NotFoundException("Không tìm thấy loại phòng.");

                // 2. Lấy danh sách Inventory cần update
                var inventories = await _inventoryRepository
                    .GetByRoomTypeIdFromDateAsync(roomTypeId, startDate);

                if (!inventories.Any()) return true; // Không có gì để update thì return luôn

                // Xác định khoảng thời gian cần lấy RoomBlock (Min Date -> Max Date)
                var minDate = inventories.Min(x => x.Date);
                var maxDate = inventories.Max(x => x.Date);

                // 3. TỐI ƯU: Lấy TẤT CẢ RoomBlock trong khoảng thời gian này MỘT LẦN DUY NHẤT
                // Lưu ý: Logic lấy block trùm lên khoảng thời gian start -> end
                var relevantBlocks = await _context.RoomBlocks
                    .Where(rb => rb.Room.RoomTypeId == roomTypeId && // Chỉ lấy block của loại phòng này
                                 rb.StartDate <= maxDate &&
                                 rb.EndDate > minDate)
                    .ToListAsync();

                // 4. Xử lý logic trong bộ nhớ (In-Memory) -> Cực nhanh
                // Lọc ra các phòng vật lý đang active (không maintenance, không deleted)
                var activePhysicalRooms = roomType.Rooms
                    .Where(r => !r.IsDeleted && r.Status != RoomStatus.Maintenance)
                    .ToList();

                foreach (var inventory in inventories)
                {
                    // Tìm các phòng bị block trong ngày cụ thể này từ danh sách đã lấy ở bước 3
                    // Chạy trong RAM nên rất nhanh
                    var blockedRoomIdsInDay = relevantBlocks
                        .Where(rb => rb.StartDate <= inventory.Date && inventory.Date < rb.EndDate)
                        .Select(rb => rb.RoomId)
                        .ToHashSet(); // Dùng HashSet để tra cứu nhanh hơn List

                    // Đếm số phòng active mà không nằm trong danh sách block
                    var totalRoomsCapable = activePhysicalRooms
                        .Count(r => !blockedRoomIdsInDay.Contains(r.Id));

                    // Cập nhật
                    inventory.TotalRooms = totalRoomsCapable;
                    inventory.AvailableRooms = Math.Max(0, totalRoomsCapable - inventory.BookedRooms);
                }

                // 5. Lưu xuống DB một lần
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<RoomInventoryDto>> GetInventoryAsync(DateOnly startDate, DateOnly endDate, int hotelId)
        {
            var roomTypes = await _roomTypeRepository.GetAllAsync(hotelId);
            if (!roomTypes.Any())
            {
                return new List<RoomInventoryDto>();
            }

            var roomTypeIds = roomTypes.Select(r => r.Id).ToList();
            var inventories = await _inventoryRepository.GetByDateRangeAndRoomTypeIdsAsync(startDate, endDate, roomTypeIds);

            // Manual mapping
            return inventories.Select(i => new RoomInventoryDto
            {
                Id = i.Id,
                RoomTypeId = i.RoomTypeId,
                RoomTypeName = i.RoomType?.RoomTypeCategory?.Name ?? "Unknown", 
                Date = i.Date,
                TotalRooms = i.TotalRooms,
                BookedRooms = i.BookedRooms,
                AvailableRooms = i.AvailableRooms
            });
        }
    }
}

