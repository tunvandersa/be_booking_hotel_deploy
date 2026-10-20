namespace do_an_epu.DTOs
{
    public class RoomTypeCategoryCreateDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class RoomTypeCategoryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class RoomTypeCategoryUpdateDto
    {
        public string Name { get; set; } = string.Empty;
    }
}
