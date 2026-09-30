using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegeHallBooking.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public int HallId { get; set; }
        [ForeignKey("HallId")]
        public virtual Hall? Hall { get; set; }

        [Required]
        public int DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Required]
        [StringLength(255)]
        public string Purpose { get; set; } = string.Empty;

        [Required]
        [Range(1, 10000)]
        public int ExpectedParticipants { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [StringLength(500)]
        public string? AdminRemarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        public string? ProcessedByUserId { get; set; }
    }
}
