using System.ComponentModel.DataAnnotations;

namespace CollegeHallBooking.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string DepartmentCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string ContactEmail { get; set; } = string.Empty;

        [StringLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        [StringLength(100)]
        public string HeadOfDepartment { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public string? UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
