using System.ComponentModel.DataAnnotations;

namespace do_an_epu.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required string Name { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = new DateTime();
        public DateTime? UpdatedAt { get; set; }
    }
}
