using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace do_an_epu.Models
{
    public class Floor
    {
        [Key]
        public int Id { get; set; }
        public virtual ICollection<Room> Rooms { get; set; }
        [Required]
        public int HotelId { get; set; }
        [ForeignKey("HotelId")]
        public virtual Hotel Hotels { get; set; } = null!;
        public int FloorNumber { get; set; }
        public Floor()
        {
            Rooms = new HashSet<Room>();
        }
    }
}
