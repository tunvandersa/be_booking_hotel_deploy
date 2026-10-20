namespace do_an_epu.DTOs
{
    public class BookingDto
    {
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Nights { get; set; }
        public List<RoomBookingItem> Rooms { get; set; } = new List<RoomBookingItem>();
        public decimal TotalRoomPrice { get; set; }
        public decimal TotalExtraAdultFee { get; set; }
        public decimal TotalExtraChildFee { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
    public class RoomBookingItem
    {
        public int Index { get; set; }
        public int RoomTypeId { get; set; }
        public string RoomTypeName { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public int Infants { get; set; }
        public decimal BasePrice { get; set; }
        public decimal ExtraAdultPrice { get; set; }
        public decimal ExtraChildPrice { get; set; }
        public decimal ExtraAdultFee { get; set; }
        public decimal ExtraChildFee { get; set; }
        public decimal TotalRoomPrice { get; set; }
    }
    public class BookingInfor
    {
        public int HotelId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public virtual List<RoomBookingInfor> Rooms { get; set; } = new List<RoomBookingInfor>();
    }
    public class RoomBookingInfor
    {
        public int RoomTypeId { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public int Infants { get; set; }
    }

    // DTO cho việc thêm phòng vào session
    public class AddRoomRequest
    {
        public int HotelId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int RoomTypeId { get; set; }
        public int RoomIndex { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public int Infants { get; set; }
    }

    // DTO cho việc xóa phòng khỏi session
    public class RemoveRoomRequest
    {
        public int RoomTypeId { get; set; }
        public int RoomIndex { get; set; }
    }

    // Session booking lưu trong memory/cache
    public class BookingSession
    {
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Nights { get; set; }
        public List<RoomBookingItem> Rooms { get; set; } = new List<RoomBookingItem>();
        public decimal TotalRoomPrice { get; set; }
        public decimal TotalExtraAdultFee { get; set; }
        public decimal TotalExtraChildFee { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime ExpiryTime { get; set; }
    }

    // DTO cho thông tin khách hàng khi checkout
    public class GuestInfo
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? SpecialRequests { get; set; }
    }

    // DTO cho request checkout
    public class CheckoutRequest
    {
        public GuestInfo GuestInfo { get; set; } = new GuestInfo();
        public string PaymentMethod { get; set; } = "PayAtHotel"; // PayAtHotel, BankTransfer, VNPay
    }

    // DTO cho response sau khi checkout thành công
    public class CheckoutResponse
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Nights { get; set; }
        public List<BookedRoomInfo> Rooms { get; set; } = new List<BookedRoomInfo>();
        public GuestInfo GuestInfo { get; set; } = new GuestInfo();
        public decimal TotalPrice { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
    }

    public class BookedRoomInfo
    {
        public string RoomTypeName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public int Adults { get; set; }
        public int Children { get; set; }
        public int Infants { get; set; }
        public decimal Price { get; set; }
    }
}
