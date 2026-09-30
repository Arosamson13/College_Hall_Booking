using System.ComponentModel.DataAnnotations;

namespace CollegeHallBooking.ViewModels
{
    public class BookingRequestViewModel
    {
        [Required(ErrorMessage = "Please select a hall")]
        [Display(Name = "Select Hall")]
        public int HallId { get; set; }

        public string? HallName { get; set; }
        public string? HallLocation { get; set; }
        public int HallCapacity { get; set; }
        public string? HallImagePath { get; set; }

        [Required(ErrorMessage = "Booking date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Start time is required")]
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; } = new TimeSpan(9, 0, 0);

        [Required(ErrorMessage = "End time is required")]
        [DataType(DataType.Time)]
        [Display(Name = "End Time")]
        public TimeSpan EndTime { get; set; } = new TimeSpan(12, 0, 0);

        [Required(ErrorMessage = "Purpose of booking is required")]
        [StringLength(255, ErrorMessage = "Purpose cannot exceed 255 characters")]
        [Display(Name = "Purpose / Event Name")]
        public string Purpose { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expected participants count is required")]
        [Range(1, 10000, ErrorMessage = "Participants count must be at least 1")]
        [Display(Name = "Expected Participants")]
        public int ExpectedParticipants { get; set; }
    }
}
