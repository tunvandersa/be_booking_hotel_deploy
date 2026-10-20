using do_an_epu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Drawing;
using System.Reflection.Emit;

namespace do_an_epu.Data
{
    public class ApplicationDbContext : IdentityDbContext<
        AppUser,
    IdentityRole<int>,
    int,
    IdentityUserClaim<int>,
    IdentityUserRole<int>,
    IdentityUserLogin<int>,
    IdentityRoleClaim<int>,
    IdentityUserToken<int>
        >
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Lấy HttpContext hiện tại của Request
            var httpContext = _httpContextAccessor.HttpContext;

            // Kiểm tra xem Middleware đã đặt TenantConnectionString vào Items chưa
            if (httpContext != null && httpContext.Items.TryGetValue("TenantConnectionString", out var connStringObj))
            {
                var connectionString = connStringObj?.ToString();
                if (!string.IsNullOrEmpty(connectionString))
                {
                    // Ép EF Core sử dụng DB của Tenant hiện tại
                    optionsBuilder.UseSqlServer(connectionString);
                }
            }

            base.OnConfiguring(optionsBuilder);
        }
        public DbSet<UpgradeTransaction> UpgradeTransactions { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<AmenityCategory> AmenityCategories { get; set; }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Floor> Floors { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<BookingDetail> BookingDetails { get; set; }
        public DbSet<HotelAmenity> HotelAmenities { get; set; }
        public DbSet<RoomAmenity> RoomAmenities { get; set; }
        public DbSet<RoomTypeCategory> RoomTypeCategory { get; set; }
        public DbSet<RoomInventory> RoomInventory { get; set; }
        public DbSet<RoomBlock> RoomBlocks { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // 1. Cấu hình cho BookingDetail (BookingId + RoomId)
            builder.Entity<BookingDetail>()
                    .HasKey(bd => bd.Id);

            // 2. Cấu hình mối quan hệ với Room là KHÔNG BẮT BUỘC (Optional)
            builder.Entity<BookingDetail>()
                .HasOne(bd => bd.Room)
                .WithMany(r => r.BookingDetails)
                .HasForeignKey(bd => bd.RoomId)
                .IsRequired(false) // <--- QUAN TRỌNG: Cho phép RoomId là null
                .OnDelete(DeleteBehavior.Restrict); // Tránh xóa nhầm

            // 2. Cấu hình cho HotelAmenity (HotelId + AmenityId)
            builder.Entity<HotelAmenity>()
                .HasKey(ha => new { ha.HotelId, ha.AmenityId });

            // 3. Cấu hình cho RoomAmenity (RoomId + AmenityId)
            builder.Entity<RoomAmenity>()
                .HasKey(ra => new { ra.RoomTypeId, ra.AmenityId });
            builder.Entity<Floor>()
               .HasOne(f => f.Hotels)
               .WithMany(h => h.Floors)
               .HasForeignKey(f => f.HotelId)
               .OnDelete(DeleteBehavior.Cascade);

            // Floors -> Rooms (❗ TẮT CASCADE)
            builder.Entity<Room>()
                .HasOne(r => r.Floor)
                .WithMany(f => f.Rooms)
                .HasForeignKey(r => r.FloorId)
                .OnDelete(DeleteBehavior.NoAction);

            //builder.Entity<RoomInventory>(entity =>
            //{
            //    entity.ToTable("RoomInventory"); 
            //    entity.Property(e => e.Date)
            //          .HasColumnType("date");

            //    entity.HasIndex(e => new { e.RoomTypeId, e.Date })
            //          .IsUnique();
            //});
        }
    }
}