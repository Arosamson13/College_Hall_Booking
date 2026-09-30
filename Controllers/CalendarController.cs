using Microsoft.AspNetCore.Mvc;
using CollegeHallBooking.Services;

namespace CollegeHallBooking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public CalendarController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetEvents(int? hallId = null)
        {
            var events = await _bookingService.GetCalendarEventsAsync(hallId);
            return Ok(events);
        }
    }
}
