namespace do_an_epu.DTOs
{
    public class AvailableUpgradeDto
    {
        public int BookingDetailId { get; set; }
        public int CurrentRoomTypeId { get; set; }
        public string CurrentRoomTypeName { get; set; }
        public decimal CurrentPrice { get; set; }
        public List<UpgradeOptionDto> Options { get; set; } = new();
    }

    public class UpgradeOptionDto
    {
        public int NewRoomTypeId { get; set; }
        public string NewRoomTypeName { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal NewPricePerNight { get; set; }
        public decimal TotalAdditionalCost { get; set; }
    }

    public class UpgradeRoomRequest
    {
        public int BookingDetailId { get; set; }
        public int NewRoomTypeId { get; set; }
    }
}
