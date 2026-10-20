using do_an_epu.DTOs;
using do_an_epu.Exceptions;
using do_an_epu.Models;
using do_an_epu.Repositories.Interface;
using do_an_epu.Services.Interface;

namespace do_an_epu.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _locationRepository;

        public LocationService(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        public async Task<LocationResponseDto> CreateLocationAsync(LocationCreateDto locationDto)
        {
            // Kiểm tra tên đã tồn tại chưa
            var exists = await _locationRepository.ExistsByNameAsync(locationDto.Name);
            if (exists)
            {
                throw new DomainException($"Địa điểm '{locationDto.Name}' đã tồn tại");
            }

            var location = new Location
            {
                Name = locationDto.Name,
                Description = locationDto.Description
            };

            var createdLocation = await _locationRepository.AddAsync(location);

            return new LocationResponseDto
            {
                Id = createdLocation.Id,
                Name = createdLocation.Name,
                Description = createdLocation.Description,
                HotelCount = 0
            };
        }

        public async Task<List<LocationResponseDto>> GetAllLocationsAsync()
        {
            var locations = await _locationRepository.GetAllAsync();
            return locations.Select(l => new LocationResponseDto
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                HotelCount = l.Hotels?.Count ?? 0
            }).ToList();
        }

        public async Task<LocationResponseDto?> GetLocationByIdAsync(int id)
        {
            var location = await _locationRepository.GetByIdAsync(id);
            if (location == null)
                return null;

            return new LocationResponseDto
            {
                Id = location.Id,
                Name = location.Name,
                Description = location.Description,
                HotelCount = location.Hotels?.Count ?? 0
            };
        }

        public async Task<LocationResponseDto> UpdateLocationAsync(int id, LocationUpdateDto locationDto)
        {
            var location = await _locationRepository.GetByIdAsync(id);
            if (location == null)
            {
                throw new NotFoundException($"Không tìm thấy địa điểm với ID {id}");
            }

            // Kiểm tra tên mới đã tồn tại chưa (ngoại trừ chính nó)
            if (location.Name.ToLower() != locationDto.Name.ToLower())
            {
                var exists = await _locationRepository.ExistsByNameAsync(locationDto.Name);
                if (exists)
                {
                    throw new DomainException($"Địa điểm '{locationDto.Name}' đã tồn tại");
                }
            }

            location.Name = locationDto.Name;
            location.Description = locationDto.Description;

            var updatedLocation = await _locationRepository.UpdateAsync(location);

            return new LocationResponseDto
            {
                Id = updatedLocation.Id,
                Name = updatedLocation.Name,
                Description = updatedLocation.Description,
                HotelCount = updatedLocation.Hotels?.Count ?? 0
            };
        }

        public async Task<bool> DeleteLocationAsync(int id)
        {
            return await _locationRepository.DeleteAsync(id);
        }
    }
}
