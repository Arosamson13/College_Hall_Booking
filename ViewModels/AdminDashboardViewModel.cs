using CollegeHallBooking.Models;

namespace CollegeHallBooking.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalHalls { get; set; }
        public int ActiveHalls { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalBookings { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedBookings { get; set; }
        public int RejectedBookings { get; set; }
        public int UpcomingReservations { get; set; }

        public List<Booking> PendingBookingList { get; set; } = new List<Booking>();
        public List<Booking> UpcomingBookingList { get; set; } = new List<Booking>();
        public List<Hall> HallsList { get; set; } = new List<Hall>();
    }
}
