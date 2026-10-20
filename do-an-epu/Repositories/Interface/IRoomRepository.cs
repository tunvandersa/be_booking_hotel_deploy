using do_an_epu.Models;

namespace do_an_epu.Repositories.Interface
{
    public interface IRoomRepository
    {
        Task<Room> AddAsync(Room room);
        Task<List<Room>> GetAllAsync(int hotelId);
        Task<List<Room>> GetByRoomTypeAsync(int roomTypeId);
        Task<int> CountActiveRoomsByRoomTypeAsync(int roomTypeId);
        Task<Room> GetByIdAsync(int id);
        Task<Room> UpdateAsync(Room room);
        Task<bool> DeleteAsync(int id);
        Task<bool> IsRoomNumberExistsAsync(int roomNumber, int hotelId);
        Task<List<Room>> GetRoomsByFloorAsync(int floorId);
        Task<List<Room>> GetAvailableRoomsAsync(int hotelId);
    }
}
