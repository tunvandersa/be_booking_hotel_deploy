using do_an_epu.Models;

namespace do_an_epu.DTOs
{
    public class BookingAdminDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string HotelName { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } // Assuming created at exists or use CheckIn
        public int NumberOfGuests { get; set; }
    }
}
