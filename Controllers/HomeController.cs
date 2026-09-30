using Microsoft.AspNetCore.Mvc;

namespace CollegeHallBooking.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Dashboard", "Admin");
                if (User.IsInRole("Department"))
                    return RedirectToAction("Dashboard", "Department");
            }

            return RedirectToAction("Login", "Account");
        }
    }
}
