using CollegeHallBooking.Models;
using CollegeHallBooking.ViewModels;

namespace CollegeHallBooking.Services
{
    public interface IBookingService
    {
        Task<bool> HasOverlapAsync(int hallId, DateTime bookingDate, TimeSpan startTime, TimeSpan endTime, int? excludeBookingId = null);
        Task<(bool Success, string Message, int? BookingId)> CreateBookingAsync(int departmentId, BookingRequestViewModel model);
        Task<bool> ApproveBookingAsync(int bookingId, string adminRemarks, string processedByUserId);
        Task<bool> RejectBookingAsync(int bookingId, string adminRemarks, string processedByUserId);
        Task<bool> CancelBookingAsync(int bookingId, int departmentId);
        Task<List<Booking>> GetBookingsAsync(BookingFilterViewModel filter);
        Task<List<Booking>> GetDepartmentBookingsAsync(int departmentId);
        Task<AdminDashboardViewModel> GetAdminDashboardStatsAsync();
        Task<DepartmentDashboardViewModel> GetDepartmentDashboardStatsAsync(int departmentId);
        Task<List<object>> GetCalendarEventsAsync(int? hallId = null);
    }
}
