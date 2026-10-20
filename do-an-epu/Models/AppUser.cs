using Microsoft.AspNetCore.Identity; 
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace do_an_epu.Models
{
    public class AppUser : IdentityUser<int>
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        public string? Avatar { get; set; }
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public AppUser()
        {
            Bookings = new HashSet<Booking>();
            Reviews = new HashSet<Review>();
        }
    }
}