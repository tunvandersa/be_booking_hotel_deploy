using do_an_epu.Models;

namespace do_an_epu.Repositories.Interface
{
    public interface IRoomTypeRepository
    {
        Task<RoomType> AddAsync(RoomType roomType);
        Task<List<RoomType>> GetAllAsync( int hotelId);
        Task<List<RoomType>> GetRoomTypesByHotelAsync(int hotelId);
        Task<RoomType> GetByIdAsync(int id);
        Task<RoomType> UpdateAsync(RoomType roomType);
        Task<bool> DeleteAsync(int id);
        Task<List<RoomTypeCategory>> GetRoomTypeCategoriesAsync();
        Task<List<RoomType>> GetRoomTypesByIdsAsync(List<int> ids);
        
        // RoomTypeCategory methods
        Task<RoomTypeCategory> AddCategoryAsync(RoomTypeCategory category);
        Task<RoomTypeCategory?> GetCategoryByIdAsync(int id);
        Task<RoomTypeCategory> UpdateCategoryAsync(RoomTypeCategory category);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> CategoryExistsByNameAsync(string name);
    }
}
