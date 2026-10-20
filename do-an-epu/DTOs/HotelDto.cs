using CloudinaryDotNet.Actions;
using do_an_epu.Models;
using System.ComponentModel.DataAnnotations;

namespace do_an_epu.DTOs
{
    public class HotelCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CheckInTime { get; set; } = string.Empty;
        public string CheckOutTime { get; set; } = string.Empty;
        public int? LocationId { get; set; }
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
        public string AmenityIds { get; set; } = string.Empty;
        public string Floors { get; set; } = string.Empty;
    }
    public class HotelResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; }
        public double Rating { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public int? LocationId { get; set; }
        public string? LocationName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public int Status { get; set; }
        public virtual List<AmenityResponseDto> Amenities { get; set; } = new List<AmenityResponseDto>();
        public virtual List<RoomTypeResponseDto> RoomTypes { get; set; } = new List<RoomTypeResponseDto>();
    }
    public class AmenityHotelDto
    {
        public int AmenityId { get; set; }
    }
    public class FloorHotelDto
    {
        public int? Id { get; set; }
        public List<int>? Rooms { get; set; }
    }

    public class HotelUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? CheckInTime { get; set; } = string.Empty;
        public string? CheckOutTime { get; set; } = string.Empty;
        public int? LocationId { get; set; }
        public int? Status { get; set; }
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
        public string ExistingImageUrls { get; set; } = string.Empty;
        public string AmenityIds { get; set; } = string.Empty;
    }
}

