using System.Linq;
using System.Threading.Tasks;
using CMCSCopilot.Data;
using CMCSCopilot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace CMCSCopilot.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AdminController(ApplicationDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var pending = await _db.Claims.Include(c => c.Files).Where(c => c.Status == ClaimStatus.Pending).OrderBy(c => c.SubmittedAt).ToListAsync();
            return View(pending);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, ClaimStatus status)
        {
            var claim = await _db.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.Status = status;
            claim.LastUpdatedBy = User?.Identity?.Name ?? "Admin";
            claim.LastUpdatedAt = System.DateTime.UtcNow;

            _db.Update(claim);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
