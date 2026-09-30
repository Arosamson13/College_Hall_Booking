using CollegeHallBooking.Models;

namespace CollegeHallBooking.ViewModels
{
    public class DepartmentDashboardViewModel
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int MyTotalBookings { get; set; }
        public int MyPendingBookings { get; set; }
        public int MyApprovedBookings { get; set; }
        public int MyRejectedBookings { get; set; }
        public bool MustChangePassword { get; set; }

        public List<Booking> MyRecentBookings { get; set; } = new List<Booking>();
        public List<Booking> MyUpcomingReservations { get; set; } = new List<Booking>();
        public List<Hall> AvailableHalls { get; set; } = new List<Hall>();
    }
}
