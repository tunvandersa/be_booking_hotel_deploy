using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace do_an_epu.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int BookingId { get; set; }
        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; } = null!;
        [Range(1, 5)]
        public int Rating { get; set; }
        [StringLength(500)]
        [Required]
        public string Comment { get; set; } = string.Empty;
        public DateTime DatePosted { get; set; } = DateTime.UtcNow;
    }
}
