using System.ComponentModel.DataAnnotations;

namespace do_an_epu.Models
{
    public class UpgradeTransaction
    {
        [Key]
        public string Id { get; set; } // TxnRef or unique ID from upgrade request
        public int BookingId { get; set; }
        public int BookingDetailId { get; set; }
        public int OldRoomTypeId { get; set; }
        public int NewRoomTypeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsPaid { get; set; } = false;
    }
}
