using System.Collections.Generic;
using System.Threading.Tasks;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.ViewModels;

namespace ContractMonthlyClaimSystem.Services
{
    public interface IClaimService
    {
        Task<Claim> SubmitClaimAsync(ClaimViewModel model, string lecturerId);
        Task<Claim> GetByIdAsync(int id);
        Task<List<Claim>> GetClaimsByLecturerIdAsync(string lecturerId);
        Task<List<Claim>> GetPendingClaimsAsync();
        Task<bool> ApproveClaimAsync(int claimId, string managerId);
        Task<bool> RejectClaimAsync(int claimId, string managerId, string rejectionReason);
        decimal CalculateTotalAmount(double hoursWorked, decimal hourlyRate);
    }
}
