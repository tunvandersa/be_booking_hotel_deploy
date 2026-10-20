using do_an_epu.Repositories.Interface;
using do_an_epu.DTOs;
using do_an_epu.Models;
using do_an_epu.Services.Interface;
using do_an_epu.Exceptions;
using do_an_epu.Services;
using AutoMapper;

namespace do_an_epu.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IMapper _mapper;
        private readonly IInventoryService _inventoryService;

        public RoomService(IRoomRepository roomRepository, IInventoryService inventoryService, IMapper mapper)
        {
            _roomRepository = roomRepository;
            _inventoryService = inventoryService;
            _mapper = mapper;
        }

        public async Task<RoomResponseDto> AddAsync(RoomCreateDto roomDto)
        {
            // Kiểm tra số phòng đã tồn tại chưa
            var exists = await _roomRepository.IsRoomNumberExistsAsync(roomDto.RoomNumber, roomDto.HotelId);
            if (exists)
                throw new InvalidOperationException($"Số phòng {roomDto.RoomNumber} đã tồn tại");

            var room = new Room
            {
                RoomNumber = roomDto.RoomNumber,
                Name = roomDto.Name,
                FloorId = roomDto.FloorId,
                RoomTypeId = roomDto.RoomTypeId,
                Status = RoomStatus.Available,
                IsActive = true
            };

            var createdRoom = await _roomRepository.AddAsync(room);
            DateOnly startDate = DateOnly.FromDateTime(DateTime.UtcNow);

            await _inventoryService.UpInventoryAsync((int)createdRoom.RoomTypeId, startDate);
            return _mapper.Map<RoomResponseDto>(createdRoom);
        }

        public async Task<List<RoomResponseDto>> GetAllAsync(int hotelId)
        {
            var rooms = await _roomRepository.GetAllAsync(hotelId);
            return _mapper.Map<List<RoomResponseDto>>(rooms);
        }

        public async Task<List<RoomResponseDto>> GetByRoomTypeAsync(int roomTypeId)
        {
            var rooms = await _roomRepository.GetByRoomTypeAsync(roomTypeId);
            return _mapper.Map<List<RoomResponseDto>>(rooms);
        }

        public async Task<RoomResponseDto> GetByIdAsync(int id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null)
                return null;

            return _mapper.Map<RoomResponseDto>(room);
        }

        public async Task<RoomResponseDto> UpdateAsync(int id, RoomUpdateDto roomDto)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null)
                throw new NotFoundException($"Phòng với ID {id} không tồn tại");

            int oldRoomTypeId = room.RoomTypeId ?? 0;

            var oldStatus = room.Status;

            _mapper.Map(roomDto, room);
            room.UpdatedAt = DateTime.UtcNow;

            var updatedRoom = await _roomRepository.UpdateAsync(room);

            DateOnly startDate = DateOnly.FromDateTime(DateTime.UtcNow);

            if (oldRoomTypeId != updatedRoom.RoomTypeId)
            {
                await _inventoryService.UpInventoryAsync(oldRoomTypeId, startDate);

                await _inventoryService.UpInventoryAsync((int)updatedRoom.RoomTypeId, startDate);
            }

            else
            {
                if (oldStatus != updatedRoom.Status)
                {
                    await _inventoryService.UpInventoryAsync((int)updatedRoom.RoomTypeId, startDate);
                }
            }

            return _mapper.Map<RoomResponseDto>(updatedRoom);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null) return false;

            var result = await _roomRepository.DeleteAsync(id);

            if (result && room.RoomTypeId.HasValue)
            {
                DateOnly startDate = DateOnly.FromDateTime(DateTime.UtcNow);
                await _inventoryService.UpInventoryAsync(room.RoomTypeId.Value, startDate);
            }
            return result;
        }

        public async Task<bool> IsRoomNumberExistsAsync(int roomNumber, int hotelId)
        {
            return await _roomRepository.IsRoomNumberExistsAsync(roomNumber, hotelId);
        }

        public async Task<List<RoomResponseDto>> GetRoomsByFloorAsync(int floorId)
        {
            var rooms = await _roomRepository.GetRoomsByFloorAsync(floorId);
            return _mapper.Map<List<RoomResponseDto>>(rooms);
        }

        public async Task<List<RoomResponseDto>> GetAvailableRoomsAsync(int hotelId)
        {
            var rooms = await _roomRepository.GetAvailableRoomsAsync(hotelId);
            return _mapper.Map<List<RoomResponseDto>>(rooms);
        }
    }
}
