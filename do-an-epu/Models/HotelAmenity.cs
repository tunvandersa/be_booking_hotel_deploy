using System.ComponentModel.DataAnnotations.Schema;

namespace do_an_epu.Models
{
    public class HotelAmenity
    {
        public int HotelId { get; set; }
        [ForeignKey(nameof(HotelId))]
        public virtual Hotel Hotel { get; set; } = null!;
        [ForeignKey(nameof(AmenityId))]
        public int AmenityId { get; set; }
        public Amenity Amenity { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public bool IsFree { get; set; } = true;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Fee { get; set; }
    }
}
