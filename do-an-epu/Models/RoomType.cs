using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace do_an_epu.Models
{
    public class RoomType
    {
        [Key]
        public int Id { get; set; }
        public int RoomTypeCategoryId { get; set; }
        [ForeignKey("RoomTypeCategoryId")]
        public virtual RoomTypeCategory RoomTypeCategory { get; set; } = null!;
        public int HotelId { get; set; }
        [ForeignKey("HotelId")]
        public virtual Hotel Hotel { get; set; } = null!;
        public double Area { get; set; }
        public int Capacity { get; set; }
        
        [Required]
        [Range(1, 5)]
        public int MaxAdults { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ExtraAdultPrice { get; set; }
        [Required]
        [Range(0, 5)]
        public int MaxChildren { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ExtraChildPrice { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PricePerNight { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public string Description { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public virtual ICollection<Room> Rooms { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<RoomAmenity> RoomAmenities { get; set; }
        public RoomType()
        {
            Rooms = new HashSet<Room>();
            Images = new HashSet<Image>();
            RoomAmenities = new HashSet<RoomAmenity>();
        }
    }
}
