using System.ComponentModel.DataAnnotations;

namespace do_an_epu.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public virtual ICollection<Hotel> Hotels { get; set; }
        public Location()
        {
            Hotels = new HashSet<Hotel>();
        }
    }
}
