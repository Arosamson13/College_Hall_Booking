using System.ComponentModel.DataAnnotations;

namespace CollegeHallBooking.ViewModels
{
    public class DepartmentFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Department code is required")]
        [StringLength(20)]
        [Display(Name = "Department Code (e.g. CS, ME, ECE)")]
        public string DepartmentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100)]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Contact Phone")]
        public string ContactPhone { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Head of Department")]
        public string HeadOfDepartment { get; set; } = string.Empty;

        [Display(Name = "Generate User Credentials")]
        public bool CreateAccount { get; set; } = true;

        [StringLength(50)]
        [Display(Name = "Initial Temporary Password")]
        public string InitialPassword { get; set; } = "Dept@123456";

        public bool IsActive { get; set; } = true;
    }
}
