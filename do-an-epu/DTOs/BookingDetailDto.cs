namespace do_an_epu.DTOs
{
    public class BookingDetailDto : BookingHistoryDto
    {
        public string GuestName { get; set; }
        public string GuestEmail { get; set; }
        public string GuestPhone { get; set; }
        public List<BookingDetailRoomDto> Rooms { get; set; }
        public DateTime BookingDate { get; set; }
    }

    public class BookingDetailRoomDto
    {
        public int Id { get; set; }
        public string RoomTypeName { get; set; }
        public int NumberOfAdult { get; set; }
        public int NumberOfChild { get; set; }
        public decimal Price { get; set; }
    }
}
