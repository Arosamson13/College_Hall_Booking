using Microsoft.EntityFrameworkCore;
using CollegeHallBooking.Data;
using CollegeHallBooking.Models;
using CollegeHallBooking.ViewModels;

namespace CollegeHallBooking.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;

        public BookingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasOverlapAsync(int hallId, DateTime bookingDate, TimeSpan startTime, TimeSpan endTime, int? excludeBookingId = null)
        {
            var targetDate = bookingDate.Date;

            var query = _context.Bookings.Where(b =>
                b.HallId == hallId &&
                b.BookingDate.Date == targetDate &&
                b.Status == BookingStatus.Approved);

            if (excludeBookingId.HasValue)
            {
                query = query.Where(b => b.Id != excludeBookingId.Value);
            }

            // Time overlap check:
            // Two time ranges [S1, E1] and [S2, E2] overlap if S1 < E2 and E1 > S2
            return await query.AnyAsync(b => startTime < b.EndTime && endTime > b.StartTime);
        }

        public async Task<(bool Success, string Message, int? BookingId)> CreateBookingAsync(int departmentId, BookingRequestViewModel model)
        {
            if (model.StartTime >= model.EndTime)
            {
                return (false, "End time must be later than start time.", null);
            }

            if (model.BookingDate.Date < DateTime.Today)
            {
                return (false, "Booking date cannot be in the past.", null);
            }

            var hall = await _context.Halls.FindAsync(model.HallId);
            if (hall == null || !hall.IsActive)
            {
                return (false, "The selected hall is inactive or does not exist.", null);
            }

            if (model.ExpectedParticipants > hall.Capacity)
            {
                return (false, $"Expected participants ({model.ExpectedParticipants}) exceed hall capacity ({hall.Capacity}).", null);
            }

            // Check if there's already an approved booking during this slot
            bool isOverlapping = await HasOverlapAsync(model.HallId, model.BookingDate, model.StartTime, model.EndTime);
            if (isOverlapping)
            {
                return (false, "The hall is already reserved for the selected date and time range.", null);
            }

            var booking = new Booking
            {
                HallId = model.HallId,
                DepartmentId = departmentId,
                BookingDate = model.BookingDate.Date,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Purpose = model.Purpose,
                ExpectedParticipants = model.ExpectedParticipants,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return (true, "Booking request submitted successfully! Pending admin approval.", booking.Id);
        }

        public async Task<bool> ApproveBookingAsync(int bookingId, string adminRemarks, string processedByUserId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null || booking.Status != BookingStatus.Pending) return false;

            // Double check overlap before approving
            bool isOverlapping = await HasOverlapAsync(booking.HallId, booking.BookingDate, booking.StartTime, booking.EndTime, booking.Id);
            if (isOverlapping)
            {
                return false;
            }

            booking.Status = BookingStatus.Approved;
            booking.AdminRemarks = adminRemarks;
            booking.ProcessedAt = DateTime.UtcNow;
            booking.ProcessedByUserId = processedByUserId;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RejectBookingAsync(int bookingId, string adminRemarks, string processedByUserId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null) return false;

            booking.Status = BookingStatus.Rejected;
            booking.AdminRemarks = adminRemarks;
            booking.ProcessedAt = DateTime.UtcNow;
            booking.ProcessedByUserId = processedByUserId;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CancelBookingAsync(int bookingId, int departmentId)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId && b.DepartmentId == departmentId);
            if (booking == null) return false;

            booking.Status = BookingStatus.Cancelled;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Booking>> GetBookingsAsync(BookingFilterViewModel filter)
        {
            var query = _context.Bookings
                .Include(b => b.Hall)
                .Include(b => b.Department)
                .AsQueryable();

            if (filter.HallId.HasValue && filter.HallId.Value > 0)
                query = query.Where(b => b.HallId == filter.HallId.Value);

            if (filter.DepartmentId.HasValue && filter.DepartmentId.Value > 0)
                query = query.Where(b => b.DepartmentId == filter.DepartmentId.Value);

            if (filter.Status.HasValue)
                query = query.Where(b => b.Status == filter.Status.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(b => b.BookingDate >= filter.StartDate.Value.Date);

            if (filter.EndDate.HasValue)
                query = query.Where(b => b.BookingDate <= filter.EndDate.Value.Date);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(b => b.Purpose.ToLower().Contains(term) ||
                                         b.Hall!.Name.ToLower().Contains(term) ||
                                         b.Department!.DepartmentName.ToLower().Contains(term));
            }

            return await query.OrderByDescending(b => b.BookingDate).ThenBy(b => b.StartTime).ToListAsync();
        }

        public async Task<List<Booking>> GetDepartmentBookingsAsync(int departmentId)
        {
            return await _context.Bookings
                .Include(b => b.Hall)
                .Include(b => b.Department)
                .Where(b => b.DepartmentId == departmentId)
                .OrderByDescending(b => b.BookingDate)
                .ThenBy(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<AdminDashboardViewModel> GetAdminDashboardStatsAsync()
        {
            var today = DateTime.Today;

            var totalHalls = await _context.Halls.CountAsync();
            var activeHalls = await _context.Halls.CountAsync(h => h.IsActive);
            var totalDepts = await _context.Departments.CountAsync();
            var totalBookings = await _context.Bookings.CountAsync();
            var pendingRequests = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending);
            var approvedBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Approved);
            var rejectedBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Rejected);
            var upcomingReservations = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Approved && b.BookingDate >= today);

            var pendingList = await _context.Bookings
                .Include(b => b.Hall)
                .Include(b => b.Department)
                .Where(b => b.Status == BookingStatus.Pending)
                .OrderBy(b => b.BookingDate)
                .Take(10)
                .ToListAsync();

            var upcomingList = await _context.Bookings
                .Include(b => b.Hall)
                .Include(b => b.Department)
                .Where(b => b.Status == BookingStatus.Approved && b.BookingDate >= today)
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.StartTime)
                .Take(10)
                .ToListAsync();

            var halls = await _context.Halls.ToListAsync();

            return new AdminDashboardViewModel
            {
                TotalHalls = totalHalls,
                ActiveHalls = activeHalls,
                TotalDepartments = totalDepts,
                TotalBookings = totalBookings,
                PendingRequests = pendingRequests,
                ApprovedBookings = approvedBookings,
                RejectedBookings = rejectedBookings,
                UpcomingReservations = upcomingReservations,
                PendingBookingList = pendingList,
                UpcomingBookingList = upcomingList,
                HallsList = halls
            };
        }

        public async Task<DepartmentDashboardViewModel> GetDepartmentDashboardStatsAsync(int departmentId)
        {
            var dept = await _context.Departments.FindAsync(departmentId);
            var today = DateTime.Today;

            var myTotal = await _context.Bookings.CountAsync(b => b.DepartmentId == departmentId);
            var myPending = await _context.Bookings.CountAsync(b => b.DepartmentId == departmentId && b.Status == BookingStatus.Pending);
            var myApproved = await _context.Bookings.CountAsync(b => b.DepartmentId == departmentId && b.Status == BookingStatus.Approved);
            var myRejected = await _context.Bookings.CountAsync(b => b.DepartmentId == departmentId && b.Status == BookingStatus.Rejected);

            var recentBookings = await _context.Bookings
                .Include(b => b.Hall)
                .Where(b => b.DepartmentId == departmentId)
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .ToListAsync();

            var upcomingReservations = await _context.Bookings
                .Include(b => b.Hall)
                .Where(b => b.DepartmentId == departmentId && b.Status == BookingStatus.Approved && b.BookingDate >= today)
                .OrderBy(b => b.BookingDate)
                .Take(5)
                .ToListAsync();

            var availableHalls = await _context.Halls.Where(h => h.IsActive).ToListAsync();

            return new DepartmentDashboardViewModel
            {
                DepartmentName = dept?.DepartmentName ?? "Department",
                MyTotalBookings = myTotal,
                MyPendingBookings = myPending,
                MyApprovedBookings = myApproved,
                MyRejectedBookings = myRejected,
                MyRecentBookings = recentBookings,
                MyUpcomingReservations = upcomingReservations,
                AvailableHalls = availableHalls
            };
        }

        public async Task<List<object>> GetCalendarEventsAsync(int? hallId = null)
        {
            var query = _context.Bookings
                .Include(b => b.Hall)
                .Include(b => b.Department)
                .Where(b => b.Status == BookingStatus.Approved || b.Status == BookingStatus.Pending);

            if (hallId.HasValue && hallId.Value > 0)
            {
                query = query.Where(b => b.HallId == hallId.Value);
            }

            var bookings = await query.ToListAsync();

            var events = new List<object>();
            foreach (var b in bookings)
            {
                var startDateTime = b.BookingDate.Date.Add(b.StartTime);
                var endDateTime = b.BookingDate.Date.Add(b.EndTime);

                string color = b.Status == BookingStatus.Approved ? "#10B981" : "#F59E0B";

                events.Add(new
                {
                    id = b.Id,
                    title = $"{b.Department?.DepartmentCode}: {b.Purpose} ({b.Hall?.Name})",
                    start = startDateTime.ToString("s"),
                    end = endDateTime.ToString("s"),
                    status = b.Status.ToString(),
                    hallName = b.Hall?.Name,
                    deptName = b.Department?.DepartmentName,
                    color = color,
                    backgroundColor = color,
                    borderColor = color
                });
            }

            return events;
        }
    }
}
