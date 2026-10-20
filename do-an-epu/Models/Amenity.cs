using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace do_an_epu.Models
{
    public class Amenity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = default!;
        public string IconUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public int AmenityCategoryId { get; set; }
        [ForeignKey(nameof(AmenityCategoryId))]
        public virtual AmenityCategory AmenityCategory { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
        public virtual ICollection<RoomAmenity> RoomAmenities { get; set; }
        public virtual ICollection<HotelAmenity> HotelAmenities { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Amenity()
        {
            RoomAmenities = new HashSet<RoomAmenity>();
            HotelAmenities = new HashSet<HotelAmenity>();
        }
    }
}
