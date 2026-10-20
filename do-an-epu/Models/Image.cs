using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 
namespace do_an_epu.Models
{
    public class Image
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int? HotelId { get; set; } 
        [ForeignKey("HotelId")] 
        public virtual Hotel Hotel { get; set; } 
        public int? RoomTypeId { get; set; }
        [ForeignKey("RoomTypeId")]
        public string? PublicId { get; set; }
        public virtual RoomType RoomType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}