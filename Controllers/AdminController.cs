using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CollegeHallBooking.Data;
using CollegeHallBooking.Models;
using CollegeHallBooking.Services;
using CollegeHallBooking.ViewModels;

namespace CollegeHallBooking.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IHallService _hallService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public AdminController(
            IBookingService bookingService,
            IHallService hallService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _bookingService = bookingService;
            _hallService = hallService;
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<IActionResult> Dashboard()
        {
            var model = await _bookingService.GetAdminDashboardStatsAsync();
            return View(model);
        }

        #region Halls Management
        public async Task<IActionResult> Halls()
        {
            var halls = await _hallService.GetAllHallsAsync();
            return View(halls);
        }

        [HttpGet]
        public IActionResult CreateHall()
        {
            return View(new HallFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHall(HallFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "halls");
            bool success = await _hallService.CreateHallAsync(model, uploadsFolder);

            if (success)
            {
                TempData["SuccessMessage"] = $"Hall '{model.Name}' created successfully!";
                return RedirectToAction(nameof(Halls));
            }

            ModelState.AddModelError(string.Empty, "An error occurred while creating the hall.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditHall(int id)
        {
            var hall = await _hallService.GetHallByIdAsync(id);
            if (hall == null) return NotFound();

            var viewModel = new HallFormViewModel
            {
                Id = hall.Id,
                Name = hall.Name,
                Location = hall.Location,
                Capacity = hall.Capacity,
                Facilities = hall.Facilities,
                Description = hall.Description,
                IsActive = hall.IsActive,
                ExistingImagePath = hall.ImagePath
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHall(HallFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "halls");
            bool success = await _hallService.UpdateHallAsync(model, uploadsFolder);

            if (success)
            {
                TempData["SuccessMessage"] = $"Hall '{model.Name}' updated successfully!";
                return RedirectToAction(nameof(Halls));
            }

            ModelState.AddModelError(string.Empty, "An error occurred while updating the hall.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHallStatus(int id)
        {
            bool success = await _hallService.ToggleHallStatusAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Hall status updated.";
            }
            return RedirectToAction(nameof(Halls));
        }
        #endregion

        #region Department Management
        public async Task<IActionResult> Departments()
        {
            var depts = await _context.Departments
                .OrderBy(d => d.DepartmentCode)
                .ToListAsync();
            return View(depts);
        }

        [HttpGet]
        public IActionResult CreateDepartment()
        {
            return View(new DepartmentFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDepartment(DepartmentFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var existingCode = await _context.Departments.AnyAsync(d => d.DepartmentCode == model.DepartmentCode);
            if (existingCode)
            {
                ModelState.AddModelError("DepartmentCode", "A department with this code already exists.");
                return View(model);
            }

            string? userId = null;
            if (model.CreateAccount)
            {
                var user = new ApplicationUser
                {
                    UserName = model.ContactEmail,
                    Email = model.ContactEmail,
                    FullName = model.DepartmentName + " Account",
                    EmailConfirmed = true,
                    MustChangePassword = true
                };

                var result = await _userManager.CreateAsync(user, model.InitialPassword);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                await _userManager.AddToRoleAsync(user, "Department");
                userId = user.Id;
            }

            var dept = new Department
            {
                DepartmentCode = model.DepartmentCode.ToUpper(),
                DepartmentName = model.DepartmentName,
                ContactEmail = model.ContactEmail,
                ContactPhone = model.ContactPhone,
                HeadOfDepartment = model.HeadOfDepartment,
                IsActive = model.IsActive,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();

            if (userId != null)
            {
                var createdUser = await _userManager.FindByIdAsync(userId);
                if (createdUser != null)
                {
                    createdUser.DepartmentId = dept.Id;
                    await _userManager.UpdateAsync(createdUser);
                }
            }

            TempData["SuccessMessage"] = $"Department '{dept.DepartmentName}' added successfully! User credentials generated.";
            return RedirectToAction(nameof(Departments));
        }
        #endregion

        #region Bookings & Approval
        public async Task<IActionResult> Bookings(BookingFilterViewModel filter)
        {
            filter.Halls = await _context.Halls.ToListAsync();
            filter.Departments = await _context.Departments.ToListAsync();
            filter.Bookings = await _bookingService.GetBookingsAsync(filter);

            return View(filter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBooking(int id, string remarks)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            bool success = await _bookingService.ApproveBookingAsync(id, remarks ?? "Approved by Admin", currentUser?.Id ?? "");

            if (success)
            {
                TempData["SuccessMessage"] = "Booking request APPROVED successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not approve booking. Conflict or invalid status detected.";
            }

            return RedirectToAction(nameof(Bookings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBooking(int id, string remarks)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            bool success = await _bookingService.RejectBookingAsync(id, remarks ?? "Rejected by Admin", currentUser?.Id ?? "");

            if (success)
            {
                TempData["SuccessMessage"] = "Booking request REJECTED.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not reject booking.";
            }

            return RedirectToAction(nameof(Bookings));
        }

        public async Task<IActionResult> Reports(DateTime? startDate, DateTime? endDate, int? hallId, int? departmentId)
        {
            var filter = new BookingFilterViewModel
            {
                StartDate = startDate ?? DateTime.Today.AddMonths(-1),
                EndDate = endDate ?? DateTime.Today.AddMonths(1),
                HallId = hallId,
                DepartmentId = departmentId,
                Halls = await _context.Halls.ToListAsync(),
                Departments = await _context.Departments.ToListAsync()
            };

            filter.Bookings = await _bookingService.GetBookingsAsync(filter);
            return View(filter);
        }
        #endregion
    }
}
