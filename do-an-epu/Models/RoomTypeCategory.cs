using System.ComponentModel.DataAnnotations;

namespace do_an_epu.Models
{
    public class RoomTypeCategory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
