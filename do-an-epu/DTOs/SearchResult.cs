namespace do_an_epu.DTOs
{
    public class SearchResult
    {
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double Rating { get; set; }
        public string? ThumbnailUrl { get; set; }
    }

    // DTO cho kết quả tìm kiếm theo Inventory (dành cho khách hàng)
    public class InventorySearchResultDto
    {
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Rating { get; set; }
        public virtual ICollection<ImageDto> HotelImage { get; set; } = new HashSet<ImageDto>();
        public decimal MinPrice { get; set; }
        public List<RoomTypeInventoryDto> AvailableRoomTypes { get; set; } = new List<RoomTypeInventoryDto>();
    }

    public class RoomTypeInventoryDto
    {
        public int RoomTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal FinalPrice { get; set; }
        public int MaxAdults { get; set; }
        public int MaxChildren { get; set; }
        public string Description { get; set; } = string.Empty;
        public virtual ICollection<ImageDto> RoomTypeImage { get; set; } = new HashSet<ImageDto>();
        public int AvailableRooms { get; set; } // Số phòng còn trống theo inventory
    }

    // DTO raw để lấy dữ liệu từ DB
    public class InventoryRawDto
    {
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string HotelAddress { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public double Rating { get; set; }
        public virtual ICollection<ImageDto> HotelImage { get; set; } = new HashSet<ImageDto>();

        public int RoomTypeId { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public decimal? DiscountPercent { get; set; }
        public int MaxAdults { get; set; }
        public int MaxChildren { get; set; }
        public string Description { get; set; } = string.Empty;
        public virtual ICollection<ImageDto> RoomTypeImage { get; set; } = new HashSet<ImageDto>();

        public DateOnly Date { get; set; }
        public int AvailableRooms { get; set; }
    }

    public class HotelSearchResultDto
    {
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Rating { get; set; }
        public virtual ICollection<ImageDto> HotelImage { get; set; } 
        public decimal MinPrice { get; set; } 
        public List<RoomTypeResultDto> AvailableRoomTypes { get; set; } = new List<RoomTypeResultDto>();
    }

    public class RoomTypeResultDto
    {
        public int RoomTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public virtual ICollection<ImageDto> RoomTypeImage { get; set; } 
        public int AvailableCount { get; set; }
        public List<int> AvaiableRoom { get; set; } = new List<int>();
    }

    public class RoomRawDto
    {
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public string HotelAddress { get; set; }
        public int LocationId { get; set; }
        public virtual ICollection<ImageDto> HotelImage { get; set; }
        public double Rating { get; set; }

        public int RoomTypeId { get; set; }
        public string RoomTypeName { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerNight { get; set; }
        public virtual ICollection<ImageDto> RoomTypeImage { get; set; }
        public int RoomId { get; set; } 
        public RoomRawDto()
        {
            HotelImage = new HashSet<ImageDto>();
            RoomTypeImage = new HashSet<ImageDto>();
        }
    }

}
