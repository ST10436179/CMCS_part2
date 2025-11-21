using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ContractMonthlyClaimSystem.Services
{
    public class ClaimService : IClaimService
    {
        private readonly ApplicationDbContext _context;

        public ClaimService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Claim> SubmitClaimAsync(ClaimViewModel model, string lecturerId)
        {
            var claim = new Claim
            {
                LecturerId = lecturerId,
                HoursWorked = model.HoursWorked,
                HourlyRate = model.HourlyRate,
                Notes = model.Notes,
                TotalAmount = CalculateTotalAmount(model.HoursWorked, model.HourlyRate),
                SubmissionDate = DateTime.UtcNow,
                Status = ClaimStatus.Pending
            };

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            return claim;
        }

        public async Task<Claim> GetByIdAsync(int id)
        {
            return await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Claim>> GetClaimsByLecturerIdAsync(string lecturerId)
        {
            return await _context.Claims
                .Where(c => c.LecturerId == lecturerId)
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();
        }

        public async Task<List<Claim>> GetPendingClaimsAsync()
        {
            return await _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.Status == ClaimStatus.Pending)
                .OrderBy(c => c.SubmissionDate)
                .ToListAsync();
        }

        public async Task<bool> ApproveClaimAsync(int claimId, string managerId)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null || claim.Status != ClaimStatus.Pending)
                return false;

            claim.Status = ClaimStatus.Approved;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectClaimAsync(int claimId, string managerId, string rejectionReason)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null || claim.Status != ClaimStatus.Pending)
                return false;

            claim.Status = ClaimStatus.Rejected;
            claim.Notes = $"{claim.Notes}\nRejection Reason: {rejectionReason}";
            await _context.SaveChangesAsync();
            return true;
        }

        public decimal CalculateTotalAmount(double hoursWorked, decimal hourlyRate)
        {
            return (decimal)hoursWorked * hourlyRate;
        }
    }
}