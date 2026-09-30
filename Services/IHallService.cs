using CollegeHallBooking.Models;
using CollegeHallBooking.ViewModels;

namespace CollegeHallBooking.Services
{
    public interface IHallService
    {
        Task<List<Hall>> GetAllHallsAsync(bool activeOnly = false);
        Task<Hall?> GetHallByIdAsync(int id);
        Task<bool> CreateHallAsync(HallFormViewModel model, string uploadsFolder);
        Task<bool> UpdateHallAsync(HallFormViewModel model, string uploadsFolder);
        Task<bool> ToggleHallStatusAsync(int id);
        Task<bool> DeleteHallAsync(int id);
    }
}
