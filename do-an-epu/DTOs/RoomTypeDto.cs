namespace do_an_epu.DTOs
{
    public class RoomTypeCreateDto
    {
        public int HotelId { get; set; }
        public int RoomTypeCategoryId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public double Area { get; set; }
        public int Capacity { get; set; }
        public int MaxAdults { get; set; }
        public int MaxChildren { get; set; }
        public decimal ExtraAdultFee { get; set; } 
        public decimal ExtraChildFee { get; set; } 
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
        public string AmenityIds { get; set; } = string.Empty;
    }

    public class RoomTypeUpdateDto
    {
        public int RoomTypeCategoryId { get; set; }
        public string Description { get; set; }
        public decimal PricePerNight { get; set; }
        public double Area { get; set; }
        public int Capacity { get; set; }
        public int MaxAdults { get; set; }
        public int MaxChildren { get; set; }
        public decimal ExtraAdultPrice { get; set; }
        public decimal ExtraChildPrice { get; set; }
        public bool IsActive { get; set; }
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
        public string ExistingImageUrls { get; set; } = string.Empty;
        public string AmenityIds { get; set; } = string.Empty;
    }

    public class RoomTypeResponseDto
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public int RoomTypeCategoryId { get; set; }
        public string RoomTypeCategoryName { get; set; }
        public string Description { get; set; }
        public decimal PricePerNight { get; set; }
        public double Area { get; set; }
        public int Capacity { get; set; }
        public int MaxAdults { get; set; }
        public int MaxChildren { get; set; }
        public decimal ExtraAdultPrice { get; set; }
        public decimal ExtraChildPrice { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ImageDto> ImageUrls { get; set; } = new List<ImageDto>();
        public List<AmenityResponseDto> Amenities { get; set; } = new List<AmenityResponseDto>();

    }
}
