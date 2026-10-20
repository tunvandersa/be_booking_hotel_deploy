namespace do_an_epu.DTOs
{
    public class RoomCreateDto
    {
        public int HotelId { get; set; }
        public int RoomNumber { get; set; }
        public string Name { get; set; }
        public int FloorId { get; set; }
        public int? RoomTypeId { get; set; }
    }

    public class RoomUpdateDto
    {
        public string? Name { get; set; }
        public int? RoomTypeId { get; set; }
        public int? Status { get; set; }
        public bool? IsActive { get; set; }
    }

    public class RoomResponseDto
    {
        public int Id { get; set; }
        public int RoomNumber { get; set; }
        public string Name { get; set; }
        public int FloorId { get; set; }
        public string FloorName { get; set; }
        public int? RoomTypeId { get; set; }
        public string RoomTypeName { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
