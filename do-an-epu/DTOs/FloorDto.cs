namespace do_an_epu.DTOs
{
    public class FloorCreateDto
    {
        public int HotelId { get; set; }
        public int FloorNumber { get; set; }
    }

    public class FloorResponseDto
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public int FloorNumber { get; set; }
        public int RoomCount { get; set; }
    }

    public class FloorUpdateDto
    {
        public int FloorNumber { get; set; }
    }
}
