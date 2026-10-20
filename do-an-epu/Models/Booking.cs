using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace do_an_epu.Models
{
    public enum BookingStatus
    {
        Pending,     // Đang chờ xử lý
        Confirmed,   // Đã xác nhận
        Cancelled,   // Đã hủy
        Completed    // Đã hoàn thành (check-out)
    }
    public class Booking
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public required virtual AppUser User { get; set; }  
        [Required]
        public DateTime CheckInDate { get; set; }
        [Required]
        public DateTime CheckOutDate { get; set; }
        [Required]
        public int NumberOfGuests { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18.2)")]
        public decimal TotalPrice { get; set; }
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        public BookingStatus Status { get; set; }
        public virtual ICollection<BookingDetail> BookingDetails { get; set; }
        public Booking()
        {
            // Khởi tạo danh sách
            BookingDetails = new HashSet<BookingDetail>();
        }
    }

}
