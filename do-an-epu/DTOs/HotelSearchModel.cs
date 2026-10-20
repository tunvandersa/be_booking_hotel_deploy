namespace do_an_epu.DTOs
{
    public class HotelSearchModel
    {
        public int LocationId { get; set; }
        public int HotelId { get; set; }
        public DateOnly Checkin { get; set; }
        public DateOnly Checkout { get; set; }
        public int Room { get; set; }
        public string? Voucher { get; set; }
    }
}
