using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace do_an_epu.Models
{
    public enum RoomStatus
    {
        Available = 0,   
        Occupied = 1,     
        Maintenance = 2,  
        Cleaning = 3,   
    }
    public class Room
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int RoomNumber { get; set; }
        public string? Name { get; set; }
        public int FloorId { get; set; }
        [ForeignKey(nameof(FloorId))]
        public Floor Floor { get; set; } = null!;
        public int? RoomTypeId { get; set; }
        [ForeignKey(nameof(RoomTypeId))]
        public RoomType RoomType { get; set; } = null!;
        [Required]
        public RoomStatus Status { get; set; } = RoomStatus.Available;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<BookingDetail> BookingDetails { get; set; }
        public virtual ICollection<RoomBlock> RoomBlocks { get; set; }
        public Room()
        {
            BookingDetails = new HashSet<BookingDetail>();
            RoomBlocks = new HashSet<RoomBlock>();
        }
    }
}
