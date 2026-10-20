using do_an_epu.DTOs;
namespace do_an_epu.Repositories.Interface
{
    public interface ISearchRepository
    {
        Task<List<RoomRawDto>> GetAvailableRoom(HotelSearchModel searchModel);
        Task<List<InventoryRawDto>> GetAvailableRoomByInventory(HotelSearchModel searchModel);
        //Task<List<RoomRawDto>> GetAvailableRoomByHotelAsync(HotelSearchModel searchModel);
    }
}
