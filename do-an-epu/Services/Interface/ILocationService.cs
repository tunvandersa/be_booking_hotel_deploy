using do_an_epu.DTOs;

namespace do_an_epu.Services.Interface
{
    public interface ILocationService
    {
        Task<LocationResponseDto> CreateLocationAsync(LocationCreateDto locationDto);
        Task<List<LocationResponseDto>> GetAllLocationsAsync();
        Task<LocationResponseDto?> GetLocationByIdAsync(int id);
        Task<LocationResponseDto> UpdateLocationAsync(int id, LocationUpdateDto locationDto);
        Task<bool> DeleteLocationAsync(int id);
    }
}
