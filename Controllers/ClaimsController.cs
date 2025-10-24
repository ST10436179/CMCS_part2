using CMCSCopilot.Data;
using CMCSCopilot.Models;
using CMCSCopilot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMCSCopilot.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;

        public ClaimsController(ApplicationDbContext db, IFileService fileService)
        {
            _db = db;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            var claims = await _db.Claims.Include(c => c.Files).OrderByDescending(c => c.SubmittedAt).ToListAsync();
            return View(claims);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim model, IFormFile[] uploads)
        {
            model.LecturerId = User?.Identity?.Name ?? "TestLecturer";
            model.Amount = decimal.Round(model.HoursWorked * model.HourlyRate, 2);
            model.SubmittedAt = DateTime.UtcNow;
            model.LastUpdatedAt = DateTime.UtcNow;
            model.LastUpdatedBy = User?.Identity?.Name ?? "Lecturer";

            ModelState.Remove(nameof(Claim.LecturerId));
            ModelState.Remove(nameof(Claim.Amount));
            ModelState.Remove(nameof(Claim.SubmittedAt));
            ModelState.Remove(nameof(Claim.LastUpdatedAt));
            ModelState.Remove(nameof(Claim.LastUpdatedBy));

            if (!TryValidateModel(model))
            {
                var errors = ModelState
                    .Where(kvp => kvp.Value.Errors.Count > 0)
                    .Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage))}");

                var msg = "Model validation failed: " + string.Join(" | ", errors);
                Console.WriteLine(msg);

                ModelState.AddModelError(string.Empty, "Submission failed. Please fix highlighted fields.");
                return View(model);
            }

            _db.Claims.Add(model);
            await _db.SaveChangesAsync();

            if (uploads != null && uploads.Length > 0)
            {
                foreach (var file in uploads)
                {
                    var result = await _fileService.SaveFileAsync(file);
                    if (result.Success)
                    {
                        _db.ClaimFiles.Add(new ClaimFile
                        {
                            ClaimId = model.Id,
                            FileName = result.OriginalFileName,
                            StoredFileName = result.StoredFileName,
                            Size = result.Size
                        });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, $"Upload error: {result.Error}");
                    }
                }
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var claim = await _db.Claims.Include(c => c.Files).FirstOrDefaultAsync(c => c.Id == id);
            if (claim == null) return NotFound();
            return View(claim);
        }
    }
}
