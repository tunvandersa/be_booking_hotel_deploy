using do_an_epu.DTOs;
using do_an_epu.Models;

namespace do_an_epu.Services.Interface
{
    public interface IRoomService
    {
        Task<RoomResponseDto> AddAsync(RoomCreateDto roomDto);
        Task<List<RoomResponseDto>> GetAllAsync(int hotelId);
        Task<List<RoomResponseDto>> GetByRoomTypeAsync(int roomTypeId);
        Task<RoomResponseDto> GetByIdAsync(int id);
        Task<RoomResponseDto> UpdateAsync(int id, RoomUpdateDto roomDto);
        Task<bool> DeleteAsync(int id);
        Task<bool> IsRoomNumberExistsAsync(int roomNumber, int hotelId);
        Task<List<RoomResponseDto>> GetRoomsByFloorAsync(int floorId);
        Task<List<RoomResponseDto>> GetAvailableRoomsAsync(int hotelId);
    }
}
