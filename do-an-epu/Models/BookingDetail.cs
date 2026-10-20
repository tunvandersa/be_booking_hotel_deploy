using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace do_an_epu.Models
{
    public class BookingDetail
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; } = null!;
        public int RoomTypeId { get; set; }
        [ForeignKey(nameof(RoomTypeId))]
        public RoomType RoomType { get; set; } = null!; 
        public int? RoomId {  get; set; }
        [ForeignKey(nameof(RoomId))]
        public Room? Room { get; set; }
        
        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PricePerNightAtBooking { get; set; }
        [Required] 
        public int NumberOfAdult {  get; set; }
        [Required]
        public int NumberOfChild { get; set; }
        [Required]
        public int NumberOfBaby { get; set; }
        public string SpecialRequests { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt {  get; set; }
    }
}
