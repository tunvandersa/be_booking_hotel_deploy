using do_an_epu.Services.Interface;
using do_an_epu.Repositories.Interface;
using do_an_epu.DTOs;
using do_an_epu.Exceptions;
namespace do_an_epu.Services
{
    public class SearchService : ISearchService
    {
        private readonly ISearchRepository _searchRepository;
        public SearchService(ISearchRepository searchRepository)
        {
            _searchRepository = searchRepository;
        }
        public async Task<List<HotelSearchResultDto>> GetAvailableRoomAsync(HotelSearchModel searchModel)
        {
            var results = await _searchRepository.GetAvailableRoom(searchModel);

            var result = results
                .GroupBy(r => r.HotelId)
                .Where(gHotel => gHotel.Count() >= searchModel.Room)
                .Select(gHotel => new HotelSearchResultDto
                {
                    HotelId = gHotel.Key,
                    HotelName = gHotel.First().HotelName,
                    Address = gHotel.First().HotelAddress,
                    Rating = gHotel.First().Rating,
                    MinPrice = gHotel.Min(x => x.PricePerNight),
                    HotelImage = gHotel.First().HotelImage,

                    AvailableRoomTypes = gHotel
                        .GroupBy(rt => rt.RoomTypeId)
                        .Select(gType => new RoomTypeResultDto
                        {
                            RoomTypeId = gType.Key,
                            TypeName = gType.First().RoomTypeName,
                            PricePerNight = gType.First().PricePerNight,
                            Capacity = gType.First().Capacity,
                            AvailableCount = gType.Count(),
                            RoomTypeImage = gType.First().RoomTypeImage,
                            AvaiableRoom = gType.Select(r => r.RoomId).ToList()
                        })
                        .OrderBy(rt => rt.PricePerNight)
                        .ToList()
                })
                .ToList();

            return result;
        }
        public async Task<List<InventorySearchResultDto>> GetAvailableRoomByInventoryAsync(HotelSearchModel searchModel)
        {
            var inventoryData = await _searchRepository.GetAvailableRoomByInventory(searchModel);

            var numberOfNights = searchModel.Checkout.DayNumber - searchModel.Checkin.DayNumber;

            var result = inventoryData
                .GroupBy(r => r.HotelId)
                .Select(gHotel =>
                {
                    var roomTypes = gHotel
                        .GroupBy(rt => rt.RoomTypeId)
                        .Select(gType =>
                        {
                            var inventoryDates = gType.Select(x => x.Date).Distinct().Count();
                            var minAvailable = inventoryDates >= numberOfNights
                                ? gType.Min(x => x.AvailableRooms)
                                : 0;

                            var firstItem = gType.First();
                            var discount = firstItem.DiscountPercent ?? 0;
                            var finalPrice = firstItem.PricePerNight * (1 - discount / 100);

                            return new RoomTypeInventoryDto
                            {
                                RoomTypeId = gType.Key,
                                TypeName = firstItem.RoomTypeName,
                                PricePerNight = firstItem.PricePerNight,
                                DiscountPercent = firstItem.DiscountPercent,
                                FinalPrice = finalPrice,
                                MaxAdults = firstItem.MaxAdults,
                                MaxChildren = firstItem.MaxChildren,
                                Description = firstItem.Description,
                                RoomTypeImage = firstItem.RoomTypeImage,
                                AvailableRooms = minAvailable
                            };
                        })
                        .Where(rt => rt.AvailableRooms > 0) // Chỉ lấy loại phòng còn trống
                        .OrderBy(rt => rt.FinalPrice)
                        .ToList();

                    // Tính tổng số phòng trống của tất cả loại phòng
                    var totalAvailableRooms = roomTypes.Sum(rt => rt.AvailableRooms);

                    return new
                    {
                        HotelId = gHotel.Key,
                        HotelName = gHotel.First().HotelName,
                        Address = gHotel.First().HotelAddress,
                        Rating = gHotel.First().Rating,
                        HotelImage = gHotel.First().HotelImage,
                        MinPrice = roomTypes.Any() ? roomTypes.Min(rt => rt.FinalPrice) : 0,
                        AvailableRoomTypes = roomTypes,
                        TotalAvailableRooms = totalAvailableRooms
                    };
                })
                .Where(h => h.TotalAvailableRooms >= searchModel.Room) // Lọc hotel có đủ phòng theo yêu cầu
                .Select(h => new InventorySearchResultDto
                {
                    HotelId = h.HotelId,
                    HotelName = h.HotelName,
                    Address = h.Address,
                    Rating = h.Rating,
                    HotelImage = h.HotelImage,
                    MinPrice = h.MinPrice,
                    AvailableRoomTypes = h.AvailableRoomTypes
                })
                .OrderBy(h => h.MinPrice)
                .ToList();

            return result;
        }
    }
}
