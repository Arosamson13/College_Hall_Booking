using Microsoft.AspNetCore.Identity;
using CollegeHallBooking.Models;

namespace CollegeHallBooking.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await dbContext.Database.EnsureCreatedAsync();

            // 1. Seed Roles
            string[] roles = new[] { "Admin", "Department" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Admin User
            var adminEmail = "admin@college.edu";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    MustChangePassword = false
                };

                var createAdminResult = await userManager.CreateAsync(adminUser, "Admin@Password123");
                if (createAdminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Seed Sample Halls if empty
            if (!dbContext.Halls.Any())
            {
                var sampleHalls = new List<Hall>
                {
                    new Hall
                    {
                        Name = "Main Auditorium",
                        Location = "Academic Block A - 1st Floor",
                        Capacity = 600,
                        Facilities = "HD Projector, Centralized AC, Dolby Audio, Stage Lighting, Podium, Wireless Mics",
                        Description = "Grand auditorium suitable for college convocations, cultural festivals, inter-college events, and guest seminars.",
                        ImagePath = "/images/sample_auditorium.jpg",
                        IsActive = true
                    },
                    new Hall
                    {
                        Name = "APJ Abdul Kalam Seminar Hall",
                        Location = "Science & Tech Building - 2nd Floor",
                        Capacity = 250,
                        Facilities = "Smart Board, AC, Video Conferencing, Surround Speakers, Wi-Fi",
                        Description = "State-of-the-art seminar hall equipped for technical workshops, department symposiums, and guest lectures.",
                        ImagePath = "/images/sample_seminar_hall.jpg",
                        IsActive = true
                    },
                    new Hall
                    {
                        Name = "Sir CV Raman Mini Conference Hall",
                        Location = "Administrative Block - Ground Floor",
                        Capacity = 80,
                        Facilities = "LED Display Screen, AC, Modular Conference Table, Executive Chairs",
                        Description = "Ideal for faculty meetings, placement interviews, department board meetings, and small group presentations.",
                        ImagePath = "/images/sample_conference_hall.jpg",
                        IsActive = true
                    },
                    new Hall
                    {
                        Name = "Open Air Theatre (OAT)",
                        Location = "Central Campus Grounds",
                        Capacity = 1500,
                        Facilities = "Outdoor Stage, High-Power PA System, Flood Lighting",
                        Description = "Spacious outdoor venue for annual cultural fests, music concerts, and sports ceremony events.",
                        ImagePath = "/images/sample_oat.jpg",
                        IsActive = true
                    }
                };

                await dbContext.Halls.AddRangeAsync(sampleHalls);
                await dbContext.SaveChangesAsync();
            }

            // 4. Seed Sample Departments if empty
            if (!dbContext.Departments.Any())
            {
                var depts = new List<(string Code, string Name, string Email, string Phone, string Head)>
                {
                    ("CSE", "Computer Science & Engineering", "cse@college.edu", "+91 9876543210", "Dr. Alan Turing"),
                    ("ECE", "Electronics & Communication", "ece@college.edu", "+91 9876543211", "Dr. Homi Bhabha"),
                    ("MECH", "Mechanical Engineering", "mech@college.edu", "+91 9876543212", "Dr. Nikola Tesla"),
                    ("MBA", "School of Management", "mba@college.edu", "+91 9876543213", "Dr. Peter Drucker")
                };

                foreach (var (code, name, email, phone, head) in depts)
                {
                    var deptUser = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = $"{name} Head",
                        EmailConfirmed = true,
                        MustChangePassword = true
                    };

                    var result = await userManager.CreateAsync(deptUser, "Dept@123456");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(deptUser, "Department");

                        var department = new Department
                        {
                            DepartmentCode = code,
                            DepartmentName = name,
                            ContactEmail = email,
                            ContactPhone = phone,
                            HeadOfDepartment = head,
                            IsActive = true,
                            UserId = deptUser.Id
                        };

                        await dbContext.Departments.AddAsync(department);
                        await dbContext.SaveChangesAsync();

                        deptUser.DepartmentId = department.Id;
                        await userManager.UpdateAsync(deptUser);
                    }
                }
            }

            // 5. Seed Sample Bookings if empty
            if (!dbContext.Bookings.Any())
            {
                var cseDept = dbContext.Departments.FirstOrDefault(d => d.DepartmentCode == "CSE");
                var eceDept = dbContext.Departments.FirstOrDefault(d => d.DepartmentCode == "ECE");
                var hall1 = dbContext.Halls.FirstOrDefault(h => h.Name.Contains("Auditorium"));
                var hall2 = dbContext.Halls.FirstOrDefault(h => h.Name.Contains("Seminar"));

                if (cseDept != null && hall1 != null)
                {
                    dbContext.Bookings.Add(new Booking
                    {
                        HallId = hall1.Id,
                        DepartmentId = cseDept.Id,
                        BookingDate = DateTime.Today.AddDays(2),
                        StartTime = new TimeSpan(10, 0, 0),
                        EndTime = new TimeSpan(13, 0, 0),
                        Purpose = "National Hackathon 2026 Opening Ceremony",
                        ExpectedParticipants = 450,
                        Status = BookingStatus.Approved,
                        AdminRemarks = "Approved by Admin. Sound system technician assigned.",
                        CreatedAt = DateTime.UtcNow.AddDays(-1),
                        ProcessedAt = DateTime.UtcNow.AddDays(-1)
                    });
                }

                if (eceDept != null && hall2 != null)
                {
                    dbContext.Bookings.Add(new Booking
                    {
                        HallId = hall2.Id,
                        DepartmentId = eceDept.Id,
                        BookingDate = DateTime.Today.AddDays(3),
                        StartTime = new TimeSpan(14, 0, 0),
                        EndTime = new TimeSpan(17, 0, 0),
                        Purpose = "VLSI Workshop & Hands-on Lab",
                        ExpectedParticipants = 180,
                        Status = BookingStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
