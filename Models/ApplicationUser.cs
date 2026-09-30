using Microsoft.AspNetCore.Identity;

namespace CollegeHallBooking.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public bool MustChangePassword { get; set; } = false;
        public int? DepartmentId { get; set; }
        public virtual Department? Department { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
