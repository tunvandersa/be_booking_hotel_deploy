using do_an_epu.DTOs;

namespace do_an_epu.Services.Interface
{
    public interface IFloorService
    {
        Task<FloorResponseDto> CreateFloorAsync(FloorCreateDto floorDto);
        Task<List<FloorResponseDto>> GetAllFloorsAsync();
        Task<List<FloorResponseDto>> GetFloorsByHotelIdAsync(int hotelId);
        Task<FloorResponseDto?> GetFloorByIdAsync(int id);
        Task<FloorResponseDto> UpdateFloorAsync(int id, FloorUpdateDto floorDto);
        Task<bool> DeleteFloorAsync(int id);
    }
}
