using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.Controllers
{
    [Authorize(Roles = "Lecturer,Admin")]
    public class ClaimsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClaimsController(AppDbContext context, IWebHostEnvironment environment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _environment = environment;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim claim, IFormFile supportingDocument)
        {
            if (ModelState.IsValid)
            {
                // Get current user
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                claim.UserId = user.Id;

                // Handle file upload
                if (supportingDocument != null && supportingDocument.Length > 0)
                {
                    // Validate file size (10MB max)
                    if (supportingDocument.Length > 10 * 1024 * 1024)
                    {
                        ModelState.AddModelError("supportingDocument", "File size must be less than 10MB.");
                        return View(claim);
                    }

                    // Validate file type
                    var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
                    var fileExtension = Path.GetExtension(supportingDocument.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("supportingDocument", "Only PDF, Word, and Excel files are allowed.");
                        return View(claim);
                    }

                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + supportingDocument.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await supportingDocument.CopyToAsync(fileStream);
                    }

                    claim.FileName = supportingDocument.FileName;
                    claim.FilePath = uniqueFileName;
                }

                _context.Add(claim);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Claim submitted successfully!";
                return RedirectToAction(nameof(SubmissionConfirmation), new { id = claim.Id });
            }
            return View(claim);
        }

        [HttpGet]
        public async Task<IActionResult> SubmissionConfirmation(int id)
        {
            var claim = await _context.Claims
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (claim == null)
            {
                return NotFound();
            }

            // Ensure user can only see their own claims unless they're admin
            var user = await _userManager.GetUserAsync(User);
            if (claim.UserId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(claim);
        }

        [HttpGet]
        [Authorize(Roles = "Lecturer,Admin")]
        public async Task<IActionResult> MyClaims()
        {
            var user = await _userManager.GetUserAsync(User);
            var claims = await _context.Claims
                .Where(c => c.UserId == user.Id)
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();

            return View(claims);
        }
    }
}
