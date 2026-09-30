using CollegeHallBooking.Models;

namespace CollegeHallBooking.ViewModels
{
    public class BookingFilterViewModel
    {
        public int? HallId { get; set; }
        public int? DepartmentId { get; set; }
        public BookingStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchTerm { get; set; }

        public List<Booking> Bookings { get; set; } = new List<Booking>();
        public List<Hall> Halls { get; set; } = new List<Hall>();
        public List<Department> Departments { get; set; } = new List<Department>();
    }
}
