using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace do_an_epu.Models
{
    public enum HotelStatus
    {
        Draft = 0,
        PendingSetup = 1,
        Ready = 2,
        Active = 3,
        Inactive = 4,
    }
    public class Hotel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên khách sạn là bắt buộc")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;
        [ForeignKey(nameof(LocationId))]
        public int LocationId { get; set; }
        public virtual Location Location { get; set; }
        [Required]
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        [Range(1, 5)]
        public double Rating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public HotelStatus Status { get; set; } = HotelStatus.Draft;
        public bool IsDeleted { get; set; } = false;
        public virtual ICollection<HotelAmenity> HotelAmenities { get; set; }
        public virtual ICollection<RoomType> RoomTypes { get; set; }
        public ICollection<Floor> Floors { get; set; }

        public Hotel()
        {
            Images = new HashSet<Image>();
            HotelAmenities = new HashSet<HotelAmenity>();
            RoomTypes = new HashSet<RoomType>();
            Floors = new HashSet<Floor>();
        }
    }
}

