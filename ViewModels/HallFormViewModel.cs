using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CollegeHallBooking.ViewModels
{
    public class HallFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hall name is required")]
        [StringLength(100)]
        [Display(Name = "Hall Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required")]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 10000, ErrorMessage = "Capacity must be between 1 and 10,000")]
        public int Capacity { get; set; }

        [Display(Name = "Facilities (comma-separated: Projector, AC, Sound System, etc.)")]
        public string Facilities { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        public string? ExistingImagePath { get; set; }

        [Display(Name = "Hall Photo")]
        public IFormFile? ImageFile { get; set; }
    }
}
