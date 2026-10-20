using do_an_epu.DTOs;
namespace do_an_epu.Services.Interface
{
    public interface ISearchService
    {
        Task<List<HotelSearchResultDto>> GetAvailableRoomAsync(HotelSearchModel searchModel);
        Task<List<InventorySearchResultDto>> GetAvailableRoomByInventoryAsync(HotelSearchModel searchModel);
        //Task<List<RoomTypeResultDto>> GetRoomTypesByHotelAsync(HotelSearchModel searchModel);
    }
}
