using System.Runtime.CompilerServices;

namespace do_an_epu.DTOs
{
    public class AmenityCategoryCreateDto
    {
        public string Name { get; set; } = default!;
        public bool IsDeleted { get; set; } = false;
    }
    public class AmenityCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public bool IsActive { get; set; }
        public List<AmenityResponseDto> Amenities { get; set; } = new();
    }
    public class AmenityCreateDto
    {
        public int? AmenityCategoryId { get; set; }
        public string? Name { get; set; }
        public IFormFile? Image { get; set; }
        public string? IconUrl { get; set; }
    }
    public class AmenityResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string IconUrl { get; set; }
        public bool IsActive { get; set; }
    }
}