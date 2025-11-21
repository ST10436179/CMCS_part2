using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ContractMonthlyClaimSystem.Data
{
    public static class SeedData
    {
        public static async Task Initialize(ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager)
        {
            // Create roles if they don't exist
            string[] roleNames = { "Lecturer", "Coordinator", "Manager", "HR" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create default users if they don't exist
            var users = new List<ApplicationUser>
    {
        new ApplicationUser {
            UserName = "lecturer@example.com",
            Email = "lecturer@example.com",
            FirstName = "John",
            LastName = "Doe",
            StaffNumber = "LEC001",
            Role = "Lecturer"
        },
        new ApplicationUser {
            UserName = "coordinator@example.com",
            Email = "coordinator@example.com",
            FirstName = "Jane",
            LastName = "Smith",
            StaffNumber = "COORD001",
            Role = "Coordinator"
        },
        new ApplicationUser {
            UserName = "manager@example.com",
            Email = "manager@example.com",
            FirstName = "Robert",
            LastName = "Johnson",
            StaffNumber = "MGR001",
            Role = "Manager"
        }
    };

            foreach (var user in users)
            {
                if (await userManager.FindByEmailAsync(user.Email) == null)
                {
                    var password = "Password123!";
                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, user.Role);
                    }
                }
            }

            // Save changes
            await context.SaveChangesAsync();
        }
    }
}