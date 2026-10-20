using do_an_epu.Data;
using do_an_epu.DTOs;
using do_an_epu.Models;
using do_an_epu.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace do_an_epu.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly ApplicationDbContext _context;

        public StatisticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardDataDto> GetAdminDashboardDataAsync(int year, int? month = null)
        {
            // 1. Validate & Calculate Date Range
            if (month.HasValue && (month < 1 || month > 12))
            {
                throw new ArgumentException("Invalid month value");
            }

            DateTime startDate = month.HasValue
                ? new DateTime(year, month.Value, 1, 0, 0, 0, DateTimeKind.Utc)
                : new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            DateTime endDate = month.HasValue
                ? startDate.AddMonths(1).AddTicks(-1)
                : startDate.AddYears(1).AddTicks(-1);

            // 2. Prepare Base Queries (Lazy Loading - Chưa chạy xuống DB ngay)
            // Query 1: Tất cả booking trong khoảng thời gian (Dùng để đếm tổng đơn, thống kê trạng thái)
            var bookingsInRangeQuery = _context.Bookings
                .Where(b => b.BookingDate >= startDate && b.BookingDate <= endDate);

            // Query 2: Chỉ booking có doanh thu (Dùng để tính tiền, vẽ biểu đồ)
            var revenueBookingsQuery = bookingsInRangeQuery
                .Where(b => b.Status == BookingStatus.Completed || b.Status == BookingStatus.Confirmed);

            // 3. Execute Summary Statistics
            // Lưu ý: DbContext không thread-safe nên ta await lần lượt, không dùng Task.WhenAll trên cùng 1 context
            var totalRevenue = await revenueBookingsQuery.SumAsync(b => b.TotalPrice);
            var totalBookings = await bookingsInRangeQuery.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalHotels = await _context.Hotels.CountAsync();

            var summary = new DashboardStatisticsDto
            {
                TotalRevenue = totalRevenue,
                TotalBookings = totalBookings,
                TotalUsers = totalUsers,
                TotalHotels = totalHotels
            };

            // 4. Chart Data Logic
            List<RevenueChartDto> chartData = new List<RevenueChartDto>();

            if (month.HasValue)
            {
                // Thống kê theo NGÀY trong tháng
                var dailyData = await revenueBookingsQuery
                    .GroupBy(b => b.BookingDate.Day)
                    .Select(g => new { Day = g.Key, Revenue = g.Sum(b => b.TotalPrice) })
                    .ToDictionaryAsync(k => k.Day, v => v.Revenue); // Dùng Dictionary để lookup nhanh hơn

                int daysInMonth = DateTime.DaysInMonth(year, month.Value);
                for (int day = 1; day <= daysInMonth; day++)
                {
                    chartData.Add(new RevenueChartDto
                    {
                        Label = $"{day:D2}/{month:D2}",
                        Revenue = dailyData.ContainsKey(day) ? dailyData[day] : 0
                    });
                }
            }
            else
            {
                // Thống kê theo THÁNG trong năm
                var monthlyData = await revenueBookingsQuery
                    .GroupBy(b => b.BookingDate.Month)
                    .Select(g => new { Month = g.Key, Revenue = g.Sum(b => b.TotalPrice) })
                    .ToDictionaryAsync(k => k.Month, v => v.Revenue);

                for (int m = 1; m <= 12; m++)
                {
                    chartData.Add(new RevenueChartDto
                    {
                        Label = $"T{m}",
                        Revenue = monthlyData.ContainsKey(m) ? monthlyData[m] : 0
                    });
                }
            }

            // 5. Booking Status Distribution
            var statusData = await bookingsInRangeQuery
                .GroupBy(b => b.Status)
                .Select(g => new BookingStatusStatsDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            return new AdminDashboardDataDto
            {
                Summary = summary,
                RevenueChart = chartData,
                BookingStats = statusData
            };
        }
    }
}