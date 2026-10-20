using do_an_epu.Repositories.Interface;
using do_an_epu.Data;
using do_an_epu.DTOs;
using do_an_epu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
namespace do_an_epu.Repositories
{
    public class SearchRepository : ISearchRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IHotelRepository _hotelRepository;
        public SearchRepository(ApplicationDbContext context, IHotelRepository hotelRepository)
        {
            _context = context;
            _hotelRepository = hotelRepository;
        }
        public async Task<List<RoomRawDto>> GetAvailableRoom(HotelSearchModel searchModel)
        {
            var reqCheckInDt = searchModel.Checkin.ToDateTime(TimeOnly.MinValue);
            var reqCheckOutDt = searchModel.Checkout.ToDateTime(TimeOnly.MinValue);
            var reqCheckIn = searchModel.Checkin;
            var reqCheckOut = searchModel.Checkout;

            var flatRoomsQuery = _context.Rooms
                .AsNoTracking()
                .AsSplitQuery()
                .Include(r => r.RoomType)
                    .ThenInclude(rt => rt.Images)
                .Include(r => r.RoomType)
                    .ThenInclude(rt => rt.Hotel)
                        .ThenInclude(h => h.Images)
                .Include(r => r.RoomType)
                    .ThenInclude(rt => rt.RoomTypeCategory)
                .Where(r => r.Status != RoomStatus.Maintenance)
                .Where(r => r.RoomType.Hotel.Status == HotelStatus.Active) // Chỉ lấy hotel đang hoạt động
                .Where(r => r.RoomType.Hotel.Id == searchModel.HotelId)
                .Where(r => !r.BookingDetails.Any(bd =>
                    bd.Booking.Status != BookingStatus.Cancelled &&
                    bd.Booking.Status != BookingStatus.Completed &&
                    (reqCheckInDt < bd.Booking.CheckOutDate && reqCheckOutDt > bd.Booking.CheckInDate)
                ))
                .Where(r => !r.RoomBlocks.Any(rb =>
                    rb.IsActive &&
                    (reqCheckIn < rb.EndDate && reqCheckOut > rb.StartDate)
                ));

            var flatRooms = await flatRoomsQuery.Select(r => new RoomRawDto
            {
                HotelId = r.RoomType.Hotel.Id,
                HotelName = r.RoomType.Hotel.Name,
                HotelAddress = r.RoomType.Hotel.Address,
                LocationId = r.RoomType.Hotel.LocationId,
                Rating = r.RoomType.Hotel.Rating,
                HotelImage = r.RoomType.Hotel.Images.Select(img => new ImageDto
                {
                    Id = img.Id,
                    Name = img.Name,
                    ImageUrl = img.ImageUrl
                }).ToList(),
                RoomTypeId = r.RoomTypeId ?? 0,
                RoomTypeName = r.RoomType.RoomTypeCategory.Name,
                Capacity = r.RoomType.MaxAdults + r.RoomType.MaxChildren,
                PricePerNight = r.RoomType.PricePerNight,
                RoomTypeImage = r.RoomType.Images.Select(img => new ImageDto
                {
                    Id = img.Id,
                    Name = img.Name,
                    ImageUrl = img.ImageUrl
                }).ToList(),
                RoomId = r.Id
            })
            .ToListAsync();

            return flatRooms;
        }

        public async Task<List<InventoryRawDto>> GetAvailableRoomByInventory(HotelSearchModel searchModel)
        {
            var reqCheckIn = searchModel.Checkin;
            var reqCheckOut = searchModel.Checkout;

            // Query lấy inventory trong khoảng thời gian check-in đến check-out
            var inventoryQuery = _context.RoomInventory
                .AsNoTracking()
                .AsSplitQuery()
                .Include(ri => ri.RoomType)
                    .ThenInclude(rt => rt.Images)
                .Include(ri => ri.RoomType)
                    .ThenInclude(rt => rt.Hotel)
                        .ThenInclude(h => h.Images)
                .Include(ri => ri.RoomType)
                    .ThenInclude(rt => rt.RoomTypeCategory)
                .Where(ri => ri.RoomType.Hotel.Status == HotelStatus.Active) // Chỉ lấy hotel đang hoạt động
                .Where(ri => ri.IsOpen) // Chỉ lấy inventory đang mở
                .Where(ri => ri.Date >= reqCheckIn && ri.Date < reqCheckOut) // Trong khoảng ngày
                .Where(ri => ri.AvailableRooms > 0); // Còn phòng trống

            if (searchModel.HotelId > 0)
            {
                inventoryQuery = inventoryQuery.Where(ri => ri.RoomType.HotelId == searchModel.HotelId);
            }

            // Nếu có Location thì lọc theo location
            if (searchModel.LocationId > 0)
            {
                inventoryQuery = inventoryQuery.Where(ri => ri.RoomType.Hotel.LocationId == searchModel.LocationId);
            }

            var inventoryData = await inventoryQuery.Select(ri => new InventoryRawDto
            {
                HotelId = ri.RoomType.Hotel.Id,
                HotelName = ri.RoomType.Hotel.Name,
                HotelAddress = ri.RoomType.Hotel.Address,
                LocationId = ri.RoomType.Hotel.LocationId,
                Rating = ri.RoomType.Hotel.Rating,
                HotelImage = ri.RoomType.Hotel.Images.Select(img => new ImageDto
                {
                    Id = img.Id,
                    Name = img.Name,
                    ImageUrl = img.ImageUrl
                }).ToList(),
                RoomTypeId = ri.RoomTypeId,
                RoomTypeName = ri.RoomType.RoomTypeCategory.Name,
                PricePerNight = ri.RoomType.PricePerNight,
                DiscountPercent = ri.DiscountPercent,
                MaxAdults = ri.RoomType.MaxAdults,
                MaxChildren = ri.RoomType.MaxChildren,
                Description = ri.RoomType.Description,
                RoomTypeImage = ri.RoomType.Images.Select(img => new ImageDto
                {
                    Id = img.Id,
                    Name = img.Name,
                    ImageUrl = img.ImageUrl
                }).ToList(),
                Date = ri.Date,
                AvailableRooms = ri.AvailableRooms
            })
            .ToListAsync();

            return inventoryData;
        }
    }
}
