using System.ComponentModel.DataAnnotations;

namespace do_an_epu.Models
{
    public class AmenityCategory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = default!;
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public virtual ICollection<Amenity> Amenities { get; set; }
        public AmenityCategory()
        {
            Amenities = new HashSet<Amenity>();
        }
    }
}
