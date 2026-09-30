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
    [Authorize(Roles = "Department")]
    public class DepartmentController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IHallService _hallService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DepartmentController(
            IBookingService bookingService,
            IHallService hallService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _bookingService = bookingService;
            _hallService = hallService;
            _context = context;
            _userManager = userManager;
        }

        private async Task<Department?> GetCurrentDepartmentAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            if (user.DepartmentId.HasValue)
            {
                return await _context.Departments.FindAsync(user.DepartmentId.Value);
            }
            return await _context.Departments.FirstOrDefaultAsync(d => d.UserId == user.Id || d.ContactEmail == user.Email);
        }

        public async Task<IActionResult> Dashboard()
        {
            var dept = await GetCurrentDepartmentAsync();
            if (dept == null) return RedirectToAction("Login", "Account");

            var stats = await _bookingService.GetDepartmentDashboardStatsAsync(dept.Id);
            return View(stats);
        }

        public async Task<IActionResult> Halls()
        {
            var halls = await _hallService.GetAllHallsAsync(activeOnly: true);
            return View(halls);
        }

        [HttpGet]
        public async Task<IActionResult> BookRequest(int? hallId)
        {
            var dept = await GetCurrentDepartmentAsync();
            if (dept == null) return RedirectToAction("Login", "Account");

            var model = new BookingRequestViewModel();

            if (hallId.HasValue && hallId.Value > 0)
            {
                var hall = await _hallService.GetHallByIdAsync(hallId.Value);
                if (hall != null)
                {
                    model.HallId = hall.Id;
                    model.HallName = hall.Name;
                    model.HallLocation = hall.Location;
                    model.HallCapacity = hall.Capacity;
                    model.HallImagePath = hall.ImagePath;
                }
            }

            ViewBag.HallsList = await _hallService.GetAllHallsAsync(activeOnly: true);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookRequest(BookingRequestViewModel model)
        {
            var dept = await GetCurrentDepartmentAsync();
            if (dept == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.HallsList = await _hallService.GetAllHallsAsync(activeOnly: true);
                return View(model);
            }

            var result = await _bookingService.CreateBookingAsync(dept.Id, model);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(MyBookings));
            }

            ModelState.AddModelError(string.Empty, result.Message);
            ViewBag.HallsList = await _hallService.GetAllHallsAsync(activeOnly: true);
            return View(model);
        }

        public async Task<IActionResult> MyBookings()
        {
            var dept = await GetCurrentDepartmentAsync();
            if (dept == null) return RedirectToAction("Login", "Account");

            var bookings = await _bookingService.GetDepartmentBookingsAsync(dept.Id);
            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var dept = await GetCurrentDepartmentAsync();
            if (dept == null) return RedirectToAction("Login", "Account");

            bool success = await _bookingService.CancelBookingAsync(id, dept.Id);
            if (success)
            {
                TempData["SuccessMessage"] = "Booking request has been cancelled.";
            }
            return RedirectToAction(nameof(MyBookings));
        }

        public async Task<IActionResult> Calendar()
        {
            ViewBag.Halls = await _hallService.GetAllHallsAsync(activeOnly: true);
            return View();
        }
    }
}
