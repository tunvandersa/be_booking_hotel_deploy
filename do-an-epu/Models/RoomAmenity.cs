using System.ComponentModel.DataAnnotations.Schema;

namespace do_an_epu.Models
{
    public class RoomAmenity
    {
        public int RoomTypeId { get; set; }
        [ForeignKey(nameof(RoomTypeId))]
        public RoomType RoomType { get; set; } = null!;
        [ForeignKey(nameof(AmenityId))]
        public int AmenityId { get; set; }
        public Amenity Amenity { get; set; } = null!;
        public bool IsDefault { get; set; }
        public bool IsFree { get; set; } = true;
        public decimal? ExtraFee { get; set; }
    }
}
