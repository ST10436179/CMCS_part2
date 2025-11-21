using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.Controllers
{
    [Authorize(Roles = "Coordinator,Manager,HR,Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(AppDbContext context, IWebHostEnvironment environment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _environment = environment;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var claims = await _context.Claims
                .Include(c => c.User)
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();
            return View(claims);
        }

        [HttpGet]
        [Authorize(Roles = "Coordinator,Manager,Admin")]
        public async Task<IActionResult> Pending()
        {
            var pendingClaims = await _context.Claims
                .Include(c => c.User)
                .Where(c => c.Status == ClaimStatus.Pending)
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();
            return View(pendingClaims);
        }

        // ... rest of the methods with proper authorization checks
    }
}
