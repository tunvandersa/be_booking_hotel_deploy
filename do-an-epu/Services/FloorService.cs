using do_an_epu.DTOs;
using do_an_epu.Exceptions;
using do_an_epu.Models;
using do_an_epu.Repositories.Interface;
using do_an_epu.Services.Interface;

namespace do_an_epu.Services
{
    public class FloorService : IFloorService
    {
        private readonly IFloorRepository _floorRepository;
        private readonly IHotelRepository _hotelRepository;

        public FloorService(IFloorRepository floorRepository, IHotelRepository hotelRepository)
        {
            _floorRepository = floorRepository;
            _hotelRepository = hotelRepository;
        }

        public async Task<FloorResponseDto> CreateFloorAsync(FloorCreateDto floorDto)
        {
            // Kiểm tra hotel có tồn tại không
            var hotel = await _hotelRepository.GetHotelByIdAsync(floorDto.HotelId);
            if (hotel == null)
            {
                throw new NotFoundException($"Không tìm thấy khách sạn với ID {floorDto.HotelId}");
            }

            // Kiểm tra tầng đã tồn tại chưa
            var exists = await _floorRepository.ExistsAsync(floorDto.HotelId, floorDto.FloorNumber);
            if (exists)
            {
                throw new DomainException($"Tầng {floorDto.FloorNumber} đã tồn tại trong khách sạn này");
            }

            var floor = new Floor
            {
                HotelId = floorDto.HotelId,
                FloorNumber = floorDto.FloorNumber
            };

            var createdFloor = await _floorRepository.AddAsync(floor);

            return new FloorResponseDto
            {
                Id = createdFloor.Id,
                HotelId = createdFloor.HotelId,
                FloorNumber = createdFloor.FloorNumber,
                RoomCount = 0
            };
        }

        public async Task<List<FloorResponseDto>> GetAllFloorsAsync()
        {
            var floors = await _floorRepository.GetAllAsync();
            return floors.Select(f => new FloorResponseDto
            {
                Id = f.Id,
                HotelId = f.HotelId,
                FloorNumber = f.FloorNumber,
                RoomCount = f.Rooms?.Count ?? 0
            }).ToList();
        }

        public async Task<List<FloorResponseDto>> GetFloorsByHotelIdAsync(int hotelId)
        {
            var floors = await _floorRepository.GetFloorsByHotelIdAsync(hotelId);
            return floors.Select(f => new FloorResponseDto
            {
                Id = f.Id,
                HotelId = f.HotelId,
                FloorNumber = f.FloorNumber,
                RoomCount = f.Rooms?.Count ?? 0
            }).ToList();
        }

        public async Task<FloorResponseDto?> GetFloorByIdAsync(int id)
        {
            var floor = await _floorRepository.GetByIdAsync(id);
            if (floor == null)
                return null;

            return new FloorResponseDto
            {
                Id = floor.Id,
                HotelId = floor.HotelId,
                FloorNumber = floor.FloorNumber,
                RoomCount = floor.Rooms?.Count ?? 0
            };
        }

        public async Task<FloorResponseDto> UpdateFloorAsync(int id, FloorUpdateDto floorDto)
        {
            var floor = await _floorRepository.GetByIdAsync(id);
            if (floor == null)
            {
                throw new NotFoundException($"Không tìm thấy tầng với ID {id}");
            }

            // Kiểm tra tầng mới đã tồn tại chưa
            if (floor.FloorNumber != floorDto.FloorNumber)
            {
                var exists = await _floorRepository.ExistsAsync(floor.HotelId, floorDto.FloorNumber);
                if (exists)
                {
                    throw new DomainException($"Tầng {floorDto.FloorNumber} đã tồn tại trong khách sạn này");
                }
            }

            floor.FloorNumber = floorDto.FloorNumber;
            var updatedFloor = await _floorRepository.UpdateAsync(floor);

            return new FloorResponseDto
            {
                Id = updatedFloor.Id,
                HotelId = updatedFloor.HotelId,
                FloorNumber = updatedFloor.FloorNumber,
                RoomCount = updatedFloor.Rooms?.Count ?? 0
            };
        }

        public async Task<bool> DeleteFloorAsync(int id)
        {
            return await _floorRepository.DeleteAsync(id);
        }
    }
}
