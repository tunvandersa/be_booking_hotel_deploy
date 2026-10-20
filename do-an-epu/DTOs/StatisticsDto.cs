namespace do_an_epu.DTOs
{
    public class DashboardStatisticsDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int TotalUsers { get; set; }
        public int TotalHotels { get; set; }
    }

    public class RevenueChartDto
    {
        public string Label { get; set; }
        public decimal Revenue { get; set; }
    }

    public class BookingStatusStatsDto
    {
        public string Status { get; set; }
        public int Count { get; set; }
    }

    public class AdminDashboardDataDto
    {
        public DashboardStatisticsDto Summary { get; set; }
        public List<RevenueChartDto> RevenueChart { get; set; }
        public List<BookingStatusStatsDto> BookingStats { get; set; }
    }
}
