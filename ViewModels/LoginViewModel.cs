using System.ComponentModel.DataAnnotations;

namespace CollegeHallBooking.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email or User ID is required")]
        [Display(Name = "Email / User ID")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
