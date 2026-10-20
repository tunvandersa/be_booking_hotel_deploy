using do_an_epu.Services.Interface;
using do_an_epu.DTOs;
using do_an_epu.Repositories.Interface;
using do_an_epu.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using do_an_epu.Exceptions;
using do_an_epu.Models;
using Microsoft.Extensions.Caching.Memory;
using do_an_epu.DTOs.VnPay;

namespace do_an_epu.Services
{
    public class BookingService : IBookingService
    {
        private readonly ILogger<BookingService> _logger;
        private readonly IHotelRepository _hotelRepository;
        private readonly IRoomTypeRepository _roomTypeRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IVnPayService _vnPayService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const int SESSION_EXPIRY_MINUTES = 30;

        public BookingService(
            ILogger<BookingService> logger, 
            IHotelRepository hotelRepository, 
            IRoomTypeRepository roomTypeRepository,
            IBookingRepository bookingRepository,
            IInventoryRepository inventoryRepository,
            ApplicationDbContext context,
            IMemoryCache cache,
            IVnPayService vnPayService,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _hotelRepository = hotelRepository;
            _roomTypeRepository = roomTypeRepository;
            _bookingRepository = bookingRepository;
            _inventoryRepository = inventoryRepository;
            _context = context;
            _cache = cache;
            _vnPayService = vnPayService;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Session Management

        private string GetSessionCacheKey(string sessionId) => $"BookingSession_{sessionId}";

        public async Task<BookingSession> GetBookingSessionAsync(string sessionId)
        {
            var cacheKey = GetSessionCacheKey(sessionId);
            if (_cache.TryGetValue(cacheKey, out BookingSession session))
            {
                return session;
            }
            return null;
        }

        public async Task<BookingSession> AddRoomToSessionAsync(string sessionId, AddRoomRequest request)
        {
            // Validate input
            if (request.CheckIn >= request.CheckOut)
                throw new ArgumentException("Ngày Check-out phải sau ngày Check-in");

            var cacheKey = GetSessionCacheKey(sessionId);
            var session = await GetBookingSessionAsync(sessionId);

            // Lấy thông tin hotel
            var hotel = await _hotelRepository.GetHotelByIdAsync(request.HotelId);
            if (hotel == null)
                throw new NotFoundException("Không tìm thấy khách sạn");

            // Lấy thông tin room type
            var roomType = await _roomTypeRepository.GetByIdAsync(request.RoomTypeId);
            if (roomType == null)
                throw new NotFoundException("Không tìm thấy loại phòng");

            // Validate số người
            if (request.Adults + request.Children > roomType.Capacity)
                throw new ArgumentException($"Số người vượt quá sức chứa của phòng (tối đa {roomType.Capacity} người)");

            int nights = (request.CheckOut - request.CheckIn).Days;

            // Tạo session mới nếu chưa có
            if (session == null)
            {
                session = new BookingSession
                {
                    HotelId = hotel.Id,
                    HotelName = hotel.Name,
                    CheckIn = request.CheckIn,
                    CheckOut = request.CheckOut,
                    Nights = nights,
                    Rooms = new List<RoomBookingItem>(),
                    ExpiryTime = DateTime.UtcNow.AddMinutes(SESSION_EXPIRY_MINUTES)
                };
            }

            // Kiểm tra xem phòng với index này đã tồn tại chưa
            var existingRoom = session.Rooms.FirstOrDefault(r => 
                r.RoomTypeId == request.RoomTypeId && r.Index == request.RoomIndex);
            
            if (existingRoom != null)
            {
                // Xóa phòng cũ nếu đã tồn tại
                session.Rooms.Remove(existingRoom);
            }

            // Tính phụ thu
            int extraAdults = Math.Max(0, request.Adults - roomType.MaxAdults);
            int extraChildren = Math.Max(0, request.Children - roomType.MaxChildren);

            var roomItem = new RoomBookingItem
            {
                Index = request.RoomIndex,
                RoomTypeId = roomType.Id,
                RoomTypeName = roomType.RoomTypeCategory?.Name,
                Adults = request.Adults,
                Children = request.Children,
                Infants = request.Infants,
                BasePrice = roomType.PricePerNight,
                ExtraAdultPrice = roomType.ExtraAdultPrice,
                ExtraChildPrice = roomType.ExtraChildPrice,
                ExtraAdultFee = extraAdults * roomType.ExtraAdultPrice * nights,
                ExtraChildFee = extraChildren * roomType.ExtraChildPrice * nights,
                TotalRoomPrice = (roomType.PricePerNight * nights) 
                    + (extraAdults * roomType.ExtraAdultPrice * nights) 
                    + (extraChildren * roomType.ExtraChildPrice * nights)
            };

            session.Rooms.Add(roomItem);

            // Tính lại tổng tiền
            CalculateSessionTotals(session);

            // Lưu vào cache
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(SESSION_EXPIRY_MINUTES));
            _cache.Set(cacheKey, session, cacheOptions);

            return session;
        }

        public async Task<BookingSession> RemoveRoomFromSessionAsync(string sessionId, RemoveRoomRequest request)
        {
            var cacheKey = GetSessionCacheKey(sessionId);
            var session = await GetBookingSessionAsync(sessionId);

            if (session == null)
            {
                return new BookingSession { Rooms = new List<RoomBookingItem>() };
            }

            // Tìm và xóa phòng theo roomTypeId và index
            var roomToRemove = session.Rooms.FirstOrDefault(r => 
                r.RoomTypeId == request.RoomTypeId && r.Index == request.RoomIndex);

            if (roomToRemove != null)
            {
                session.Rooms.Remove(roomToRemove);
            }

            // Tính lại tổng tiền
            CalculateSessionTotals(session);

            // Lưu lại vào cache
            if (session.Rooms.Any())
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(SESSION_EXPIRY_MINUTES));
                _cache.Set(cacheKey, session, cacheOptions);
            }
            else
            {
                _cache.Remove(cacheKey);
            }

            return session;
        }

        public async Task ClearBookingSessionAsync(string sessionId)
        {
            var cacheKey = GetSessionCacheKey(sessionId);
            _cache.Remove(cacheKey);
        }

        private void CalculateSessionTotals(BookingSession session)
        {
            session.TotalRoomPrice = session.Rooms.Sum(r => r.BasePrice * session.Nights);
            session.TotalExtraAdultFee = session.Rooms.Sum(r => r.ExtraAdultFee);
            session.TotalExtraChildFee = session.Rooms.Sum(r => r.ExtraChildFee);
            session.TotalPrice = session.TotalRoomPrice + session.TotalExtraAdultFee + session.TotalExtraChildFee;
        }

        #endregion

        public async Task<BookingDto> CreateBookingAsync(BookingInfor booking)
        {
            // 1. Validate input
            if (booking == null)
                throw new ArgumentNullException(nameof(booking));

            if (booking.CheckIn >= booking.CheckOut)
                throw new ArgumentException("Ngày Check-out phải sau ngày Check-in");

            if (booking.Rooms == null || !booking.Rooms.Any())
                throw new ArgumentException("Danh sách phòng không được rỗng");

            int nights = (booking.CheckOut - booking.CheckIn).Days;

            // 2. Lấy hotel
            var hotel = await _hotelRepository.GetHotelByIdAsync(booking.HotelId);
            if (hotel == null)
                throw new NotFoundException("Không tìm thấy khách sạn");

            // 3. Lấy tất cả RoomType cần thiết (1 query)
            var roomTypeIds = booking.Rooms.Select(r => r.RoomTypeId).Distinct().ToList();
            var roomTypes = await _roomTypeRepository.GetRoomTypesByIdsAsync(roomTypeIds);

            var roomTypeDict = roomTypes.ToDictionary(x => x.Id);

            var bookingItems = new List<RoomBookingItem>();
            decimal totalRoomPrice = 0;
            decimal totalExtraAdultFee = 0;
            decimal totalExtraChildFee = 0;

            int index = 0;
            foreach (var room in booking.Rooms)
            {
                if (!roomTypeDict.TryGetValue(room.RoomTypeId, out var roomType))
                    throw new KeyNotFoundException($"Không tìm thấy loại phòng ID {room.RoomTypeId}");

                if (room.Adults + room.Children > roomType.Capacity)
                    throw new ArgumentException($"Số người vượt quá sức chứa của {roomType.RoomTypeCategory.Name}");

                int extraAdults = Math.Max(0, room.Adults - roomType.MaxAdults);
                int extraChildren = Math.Max(0, room.Children - roomType.MaxChildren);

                var extraAdultFee = extraAdults * roomType.ExtraAdultPrice * nights;
                var extraChildFee = extraChildren * roomType.ExtraChildPrice * nights;
                var roomBasePrice = roomType.PricePerNight * nights;

                var bookingItem = new RoomBookingItem
                {
                    Index = index++,
                    RoomTypeId = roomType.Id,
                    RoomTypeName = roomType.RoomTypeCategory.Name,
                    Adults = room.Adults,
                    Children = room.Children,
                    Infants = room.Infants,
                    BasePrice = roomType.PricePerNight,
                    ExtraAdultPrice = roomType.ExtraAdultPrice,
                    ExtraChildPrice = roomType.ExtraChildPrice,
                    ExtraAdultFee = extraAdultFee,
                    ExtraChildFee = extraChildFee,
                    TotalRoomPrice = roomBasePrice + extraAdultFee + extraChildFee
                };

                totalRoomPrice += roomBasePrice;
                totalExtraAdultFee += extraAdultFee;
                totalExtraChildFee += extraChildFee;

                bookingItems.Add(bookingItem);
            }

            return new BookingDto
            {
                HotelId = hotel.Id,
                HotelName = hotel.Name,
                CheckIn = booking.CheckIn,
                CheckOut = booking.CheckOut,
                Nights = nights,
                Rooms = bookingItems,
                TotalRoomPrice = totalRoomPrice,
                TotalExtraAdultFee = totalExtraAdultFee,
                TotalExtraChildFee = totalExtraChildFee,
                TotalPrice = totalRoomPrice + totalExtraAdultFee + totalExtraChildFee
            };
        }

        #region Checkout

        public async Task<CheckoutResponse> CheckoutAsync(string sessionId, CheckoutRequest request, int? userId)
        {
            var session = await GetBookingSessionAsync(sessionId);
            if (session == null || !session.Rooms.Any())
            {
                throw new ArgumentException("Không có phòng nào trong giỏ đặt phòng");
            }

            if (string.IsNullOrWhiteSpace(request.GuestInfo.FullName))
                throw new ArgumentException("Vui lòng nhập họ tên");
            if (string.IsNullOrWhiteSpace(request.GuestInfo.Email))
                throw new ArgumentException("Vui lòng nhập email");
            if (string.IsNullOrWhiteSpace(request.GuestInfo.Phone))
                throw new ArgumentException("Vui lòng nhập số điện thoại");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var roomGroups = session.Rooms
                    .GroupBy(r => r.RoomTypeId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var bookingDetails = new List<BookingDetail>();
                var bookedRoomInfos = new List<BookedRoomInfo>();

                var checkInDate = DateOnly.FromDateTime(session.CheckIn);
                var checkOutDate = DateOnly.FromDateTime(session.CheckOut);

                foreach (var group in roomGroups)
                {
                    var roomTypeId = group.Key;
                    var roomsNeeded = group.Value;
                    var roomCount = roomsNeeded.Count;

                    var availableCount = await _inventoryRepository.GetMinAvailableRoomsAsync(
                        roomTypeId, 
                        checkInDate, 
                        checkOutDate);

                    if (availableCount < roomCount)
                    {
                        var roomTypeName = roomsNeeded.First().RoomTypeName;
                        throw new ArgumentException($"Không đủ phòng {roomTypeName} trống. Chỉ còn {availableCount} phòng.");
                    }

                    foreach (var roomInfo in roomsNeeded)
                    {
                        bookingDetails.Add(new BookingDetail
                        {
                            RoomTypeId = roomTypeId,
                            RoomId = null, // Sẽ được gán khi check-in
                            PricePerNightAtBooking = roomInfo.BasePrice,
                            NumberOfAdult = roomInfo.Adults,
                            NumberOfChild = roomInfo.Children,
                            NumberOfBaby = roomInfo.Infants,
                            SpecialRequests = request.GuestInfo.SpecialRequests ?? "",
                            BookingDate = DateTime.Now
                        });

                        bookedRoomInfos.Add(new BookedRoomInfo
                        {
                            RoomTypeName = roomInfo.RoomTypeName ?? "",
                            RoomNumber = "Chưa xếp phòng", 
                            Children = roomInfo.Children,
                            Infants = roomInfo.Infants,
                            Price = roomInfo.TotalRoomPrice
                        });
                    }

                    await _inventoryRepository.UpdateBookedRoomsAsync(
                        roomTypeId, 
                        checkInDate, 
                        checkOutDate, 
                        roomCount);
                }

                // 8. Tạo booking
                var booking = new Booking
                {
                    UserId = userId ?? 1, // Default user nếu chưa đăng nhập
                    User = null!, // Sẽ được EF load
                    CheckInDate = session.CheckIn,
                    CheckOutDate = session.CheckOut,
                    NumberOfGuests = session.Rooms.Sum(r => r.Adults + r.Children),
                    TotalPrice = session.TotalPrice,
                    BookingDate = DateTime.Now,
                    Status = request.PaymentMethod == "PayAtHotel" ? BookingStatus.Confirmed : BookingStatus.Pending,
                    BookingDetails = bookingDetails
                };

                // 9. Lưu booking vào database
                var createdBooking = await _bookingRepository.CreateAsync(booking);

                // 10. Commit transaction
                await transaction.CommitAsync();

                // 11. Xóa session sau khi checkout thành công (Non-transactional)
                // Note: If this fails, the booking is already created. user might see error but booking exists.
                // ideally we handle this gracefully.
                try 
                {
                    await ClearBookingSessionAsync(sessionId);
                } 
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error clearing booking session after checkout");
                    // swallow error, as checkout is successful
                }

                string vnPayUrl = "";
                if (request.PaymentMethod == "VnPay")
                {
                    var vnPayModel = new PaymentInformationModel
                    {
                        Amount = (double)session.TotalPrice,
                        OrderDescription = $"Thanh toan don hang {createdBooking.Id}",
                        OrderType = "other",
                        Name = request.GuestInfo.FullName ?? "Guest",
                        BookingId = createdBooking.Id.ToString()
                    };
                    vnPayUrl = _vnPayService.CreatePaymentUrl(_httpContextAccessor.HttpContext, vnPayModel);
                }

                // 12. Trả về response
                return new CheckoutResponse
                {
                    BookingId = createdBooking.Id,
                    BookingCode = $"BK{createdBooking.Id:D6}",
                    Status = createdBooking.Status.ToString(),
                    HotelName = session.HotelName,
                    CheckIn = session.CheckIn,
                    CheckOut = session.CheckOut,
                    Nights = session.Nights,
                    Rooms = bookedRoomInfos,
                    GuestInfo = request.GuestInfo,
                    TotalPrice = session.TotalPrice,
                    PaymentMethod = request.PaymentMethod,
                    PaymentUrl = vnPayUrl,
                    BookingDate = createdBooking.BookingDate
                };
            }
            catch
            {
                try
                {
                     await transaction.RollbackAsync();
                }
                catch (InvalidOperationException)
                {
                    
                }
                throw;
            }
        }

        public async Task<PaymentResponseModel> ProcessPaymentCallback(IQueryCollection collections)
        {
            var response = _vnPayService.PaymentExecute(collections);

            // Handle UPGRADE transaction
            if (response.OrderDescription != null && response.OrderDescription.StartsWith("UPGRADE:"))
            {
                 var txnId = response.OrderDescription.Substring("UPGRADE:".Length);
                 var txn = await _context.UpgradeTransactions.FirstOrDefaultAsync(t => t.Id == txnId);
                 
                 if (txn != null && !txn.IsPaid)
                 {
                     if (response.Success && response.VnPayResponseCode == "00")
                     {
                         // Process Upgrade Logic Here
                         txn.IsPaid = true;
                         // Execute Room Swap
                         using var transaction = await _context.Database.BeginTransactionAsync();
                         try 
                         {
                             var booking = await _bookingRepository.GetByIdAsync(txn.BookingId);
                             var detail = booking.BookingDetails.FirstOrDefault(bd => bd.Id == txn.BookingDetailId);
                             
                             if (booking != null && detail != null)
                             {
                                 var checkInDate = DateOnly.FromDateTime(booking.CheckInDate);
                                 var checkOutDate = DateOnly.FromDateTime(booking.CheckOutDate);
                                 // -1 old room
                                 await _inventoryRepository.UpdateBookedRoomsAsync(txn.OldRoomTypeId, checkInDate, checkOutDate, -1);
                                 // +1 new room
                                 await _inventoryRepository.UpdateBookedRoomsAsync(txn.NewRoomTypeId, checkInDate, checkOutDate, 1);
                                 
                                 detail.RoomTypeId = txn.NewRoomTypeId;
                                 var newRoomType = await _context.RoomTypes.FindAsync(txn.NewRoomTypeId);
                                 detail.PricePerNightAtBooking = newRoomType.PricePerNight;
                                 
                                 booking.TotalPrice += txn.Amount;
                                 
                                 _context.BookingDetails.Update(detail);
                                 await _bookingRepository.UpdateAsync(booking);
                                 await _context.SaveChangesAsync();
                                 await transaction.CommitAsync();
                                 
                                 // Update OrderId to return real BookingId for Frontend Redirect
                                 response.OrderId = txn.BookingId.ToString();
                             }
                         }
                         catch
                         {
                             await transaction.RollbackAsync();
                             // Log error?
                         }
                     }
                 }
                 // Even if paid already or failed logic inside, we return response. 
                 // If success=true, ensure OrderId is friendly if possible.
                 var parts = txnId.Split('_');
                 if (parts.Length >= 2) response.OrderId = parts[1];
                 
                 return response;
            }

            // FORCE check: If VerifySignature is true (Success=true), but ResponseCode != 00, treat as FAIL.
            if (response.Success && response.VnPayResponseCode != "00")
            {

            }

            if (int.TryParse(response.OrderId, out int bookingId))
            {
                var booking = await _bookingRepository.GetByIdAsync(bookingId);

                if (booking != null)
                {
                    if (response.Success && response.VnPayResponseCode == "00")
                    {
                        // Payment Success
                        booking.Status = BookingStatus.Confirmed;
                        await _bookingRepository.UpdateAsync(booking);
                    }
                    else
                    {
                        // Payment Failed OR Cancelled (Code 24, etc.)
                        // 1. Revert Inventory
                        var checkInDate = DateOnly.FromDateTime(booking.CheckInDate);
                        var checkOutDate = DateOnly.FromDateTime(booking.CheckOutDate);

                        // Group booking details by RoomTypeId
                        var roomGroups = booking.BookingDetails
                            .GroupBy(bd => bd.RoomTypeId);

                        foreach (var group in roomGroups)
                        {
                            var roomTypeId = group.Key;
                            var roomCount = group.Count();

                            // Decrease booked rooms (pass negative count)
                            await _inventoryRepository.UpdateBookedRoomsAsync(
                                roomTypeId,
                                checkInDate,
                                checkOutDate,
                                -roomCount
                            );
                        }

                        // 2. Delete Booking
                        await _bookingRepository.DeleteAsync(booking);
                    }
                }
            }
            
            // Ensure final response indicates failure if not 00
            if (response.VnPayResponseCode != "00")
            {
                response.Success = false;
            }

            return response;
        }

        public async Task<List<BookingHistoryDto>> GetBookingsByUserIdAsync(int userId)
        {
            var bookings = await _bookingRepository.GetByUserIdAsync(userId);
            
            return bookings.Select(b => new BookingHistoryDto
            {
                Id = b.Id,
                HotelName = b.BookingDetails.FirstOrDefault()?.RoomType?.Hotel?.Name ?? "Unknown Hotel",
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                TotalPrice = b.TotalPrice,
                Status = b.Status.ToString()
            }).ToList();
        }

        public async Task<bool> CancelBookingAsync(int bookingId, int userId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null) throw new NotFoundException("Booking not found");
            
            if (booking.UserId != userId) throw new UnauthorizedAccessException("Not authorized to cancel this booking");

            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
            {
                throw new InvalidOperationException("Cannot cancel booking in current status");
            }

            // 1. Revert Inventory
            var checkInDate = DateOnly.FromDateTime(booking.CheckInDate);
            var checkOutDate = DateOnly.FromDateTime(booking.CheckOutDate);

            var roomGroups = booking.BookingDetails.GroupBy(bd => bd.RoomTypeId);
            foreach (var group in roomGroups)
            {
                var roomTypeId = group.Key;
                var roomCount = group.Count();
                await _inventoryRepository.UpdateBookedRoomsAsync(roomTypeId, checkInDate, checkOutDate, -roomCount);
            }

            booking.Status = BookingStatus.Cancelled;
            await _bookingRepository.UpdateAsync(booking);
            return true;
        }

        public async Task<BookingDetailDto> GetBookingDetailAsync(int bookingId, int userId)
        {
             var booking = await _bookingRepository.GetByIdAsync(bookingId);
             if (booking == null) throw new NotFoundException("Booking not found");
             if (booking.UserId != userId) throw new UnauthorizedAccessException("Not authorized");

             return new BookingDetailDto {
                 Id = booking.Id,
                 HotelName = booking.BookingDetails.FirstOrDefault()?.RoomType?.Hotel?.Name ?? "Unknown Hotel",
                 CheckInDate = booking.CheckInDate,
                 CheckOutDate = booking.CheckOutDate,
                 TotalPrice = booking.TotalPrice,
                 Status = booking.Status.ToString(),
                 BookingDate = booking.BookingDate,
                 GuestName = $"{booking.User?.LastName} {booking.User?.FirstName}",
                 GuestEmail = booking.User?.Email,
                 GuestPhone = booking.User?.PhoneNumber,
                 Rooms = booking.BookingDetails.Select(bd => new BookingDetailRoomDto {
                     Id = bd.Id,
                     RoomTypeName = bd.RoomType?.RoomTypeCategory?.Name ?? "Unknown",
                     NumberOfAdult = bd.NumberOfAdult,
                     NumberOfChild = bd.NumberOfChild,
                     Price = bd.PricePerNightAtBooking
                 }).ToList()
             };
        }

        #endregion

        #region Upgrade

        public async Task<AvailableUpgradeDto> GetAvailableUpgradesAsync(int bookingId, int bookingDetailId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null) throw new NotFoundException("Booking not found");

            var detail = booking.BookingDetails.FirstOrDefault(bd => bd.Id == bookingDetailId);
            if (detail == null) throw new NotFoundException("Booking detail not found");

            // Only allow upgrade for Confirmed/Pending bookings, not cancelled or checked out
            //if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.CheckedOut || booking.Status == BookingStatus.CheckedIn)
            //{
            //     throw new InvalidOperationException("Cannot upgrade booking in current status");
            //}
            if(booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
            {
                throw new InvalidOperationException("Cannot upgrade booking in current status");
            };

            var currentRoomType = detail.RoomType;
            var hotelId = currentRoomType.HotelId;

            var allRoomTypes = _context.RoomTypes
                .Where(rt => rt.HotelId == hotelId && rt.IsActive && !rt.IsDeleted && rt.PricePerNight > currentRoomType.PricePerNight)
                .OrderBy(rt => rt.PricePerNight)
                .ToList();

            var checkInDate = DateOnly.FromDateTime(booking.CheckInDate);
            var checkOutDate = DateOnly.FromDateTime(booking.CheckOutDate);
            var nights = (int)(checkOutDate.ToDateTime(TimeOnly.MinValue) - checkInDate.ToDateTime(TimeOnly.MinValue)).TotalDays;
            if (nights < 1) nights = 1;

            var result = new AvailableUpgradeDto
            {
                BookingDetailId = detail.Id,
                CurrentRoomTypeId = detail.RoomTypeId,
                CurrentRoomTypeName = detail.RoomType.RoomTypeCategory?.Name ?? "Unknown",
                CurrentPrice = detail.PricePerNightAtBooking,
                Options = new List<UpgradeOptionDto>()
            };

            foreach (var rt in allRoomTypes)
            {
                // Check if RT matches capacity requirements (optional, but good practice)
                if (rt.MaxAdults < detail.NumberOfAdult) continue;

                // Check Inventory
                int available = await _inventoryRepository.GetMinAvailableRoomsAsync(rt.Id, checkInDate, checkOutDate);
                if (available > 0)
                {
                    decimal priceDiff = rt.PricePerNight - detail.PricePerNightAtBooking;
                    if (priceDiff > 0)
                    {
                        result.Options.Add(new UpgradeOptionDto
                        {
                            NewRoomTypeId = rt.Id,
                            NewRoomTypeName = _context.RoomTypeCategory.Find(rt.RoomTypeCategoryId)?.Name ?? "Upgrade",
                            Description = rt.Description,
                            ImageUrl = rt.Images.FirstOrDefault()?.ImageUrl ?? "",
                            NewPricePerNight = rt.PricePerNight,
                            TotalAdditionalCost = priceDiff * nights
                        });
                    }
                }
            }

            return result;
        }

        public async Task<string> UpgradeRoomAsync(int bookingId, UpgradeRoomRequest request)
        {
             // 0. Checks
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null) throw new NotFoundException("Booking not found");

            var detail = booking.BookingDetails.FirstOrDefault(bd => bd.Id == request.BookingDetailId);
            if (detail == null) throw new NotFoundException("Booking Detail not found");

            if (booking.Status != BookingStatus.Confirmed && booking.Status != BookingStatus.Pending)
                throw new InvalidOperationException("Only Confirmed or Pending bookings can be upgraded.");

            var checkInDate = DateOnly.FromDateTime(booking.CheckInDate);
            var checkOutDate = DateOnly.FromDateTime(booking.CheckOutDate);
            var nights = (int)(checkOutDate.ToDateTime(TimeOnly.MinValue) - checkInDate.ToDateTime(TimeOnly.MinValue)).TotalDays;
            if (nights < 1) nights = 1;

            var newRoomType = await _context.RoomTypes.FindAsync(request.NewRoomTypeId);
            if (newRoomType == null) throw new NotFoundException("New Room Type not found");

            // Inventory Check
            int available = await _inventoryRepository.GetMinAvailableRoomsAsync(newRoomType.Id, checkInDate, checkOutDate);
            if (available <= 0) throw new InvalidOperationException("Phòng hết chỗ trong giai đoạn này.");

            // Calc Diff
            decimal oldPricePerNight = detail.PricePerNightAtBooking;
            decimal newPricePerNight = newRoomType.PricePerNight;
            decimal diffPerNight = newPricePerNight - oldPricePerNight;
            decimal totalDiff = diffPerNight * nights;

            if (totalDiff <= 0) throw new InvalidOperationException("Không thể nâng cấp xuống hạng thấp hơn hoặc bằng giá.");

            // 1. Create Transaction Record
            var txnId = $"{DateTime.Now.Ticks}_{bookingId}_{request.BookingDetailId}";
            var txn = new UpgradeTransaction
            {
                Id = txnId,
                BookingId = bookingId,
                BookingDetailId = request.BookingDetailId,
                OldRoomTypeId = detail.RoomTypeId,
                NewRoomTypeId = request.NewRoomTypeId,
                Amount = totalDiff,
                IsPaid = false
            };
            _context.UpgradeTransactions.Add(txn);
            await _context.SaveChangesAsync();

            // 2. Generate VnPay URL
            var vnPayModel = new PaymentInformationModel
            {
                Amount = (double)totalDiff,
                OrderDescription = $"UPGRADE:{txnId}", 
                OrderType = "other",
                Name = booking.User?.LastName + " " + booking.User?.FirstName ?? "Guest",
                BookingId = txnId 
            };
            
            return _vnPayService.CreatePaymentUrl(_httpContextAccessor.HttpContext, vnPayModel);
        }

        #endregion
    }
}