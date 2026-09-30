using Microsoft.EntityFrameworkCore;
using CollegeHallBooking.Data;
using CollegeHallBooking.Models;
using CollegeHallBooking.ViewModels;

namespace CollegeHallBooking.Services
{
    public class HallService : IHallService
    {
        private readonly ApplicationDbContext _context;

        public HallService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Hall>> GetAllHallsAsync(bool activeOnly = false)
        {
            var query = _context.Halls.AsQueryable();
            if (activeOnly)
            {
                query = query.Where(h => h.IsActive);
            }
            return await query.OrderBy(h => h.Name).ToListAsync();
        }

        public async Task<Hall?> GetHallByIdAsync(int id)
        {
            return await _context.Halls.FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<bool> CreateHallAsync(HallFormViewModel model, string uploadsFolder)
        {
            string? imagePath = null;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                Directory.CreateDirectory(uploadsFolder);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }
                imagePath = "/uploads/halls/" + uniqueFileName;
            }

            var hall = new Hall
            {
                Name = model.Name,
                Location = model.Location,
                Capacity = model.Capacity,
                Facilities = model.Facilities,
                Description = model.Description,
                IsActive = model.IsActive,
                ImagePath = imagePath,
                CreatedAt = DateTime.UtcNow
            };

            _context.Halls.Add(hall);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateHallAsync(HallFormViewModel model, string uploadsFolder)
        {
            var hall = await _context.Halls.FindAsync(model.Id);
            if (hall == null) return false;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                Directory.CreateDirectory(uploadsFolder);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }
                hall.ImagePath = "/uploads/halls/" + uniqueFileName;
            }

            hall.Name = model.Name;
            hall.Location = model.Location;
            hall.Capacity = model.Capacity;
            hall.Facilities = model.Facilities;
            hall.Description = model.Description;
            hall.IsActive = model.IsActive;

            _context.Halls.Update(hall);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleHallStatusAsync(int id)
        {
            var hall = await _context.Halls.FindAsync(id);
            if (hall == null) return false;

            hall.IsActive = !hall.IsActive;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteHallAsync(int id)
        {
            var hall = await _context.Halls.FindAsync(id);
            if (hall == null) return false;

            // Soft delete or check if bookings exist
            var hasBookings = await _context.Bookings.AnyAsync(b => b.HallId == id);
            if (hasBookings)
            {
                hall.IsActive = false; // deactivate if bookings exist
            }
            else
            {
                _context.Halls.Remove(hall);
            }

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
