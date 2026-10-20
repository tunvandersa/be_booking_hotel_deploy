using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient.DataClassification;

namespace do_an_epu.Models
{
    public class RoomInventory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int RoomTypeId { get; set; }

        [ForeignKey(nameof(RoomTypeId))]
        public virtual RoomType RoomType { get; set; } = null!;

        [Required]
        public DateOnly Date { get; set; }
        public int TotalRooms { get; set; }
        public int BlockRooms { get; set; }
        public int BookedRooms { get; set; }
        public int AvailableRooms { get; set; }
        [Range(0,100)]
        public decimal? DiscountPercent { get; set; }
        public bool IsOpen { get; set; } = true;
    }
}
