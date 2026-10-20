namespace do_an_epu.DTOs
{
    public class RoomInventoryDto
    {
        public int Id { get; set; }
        public int RoomTypeId { get; set; }
        public string? RoomTypeName { get; set; }
        public DateOnly Date { get; set; }
        public int TotalRooms { get; set; }
        public int BookedRooms { get; set; }
        public int AvailableRooms { get; set; }
    }
}
