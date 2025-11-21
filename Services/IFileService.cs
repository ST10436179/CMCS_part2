using Microsoft.AspNetCore.Http;
using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.Services
{
    public interface IFileService
    {
        Task<Document> UploadFileAsync(IFormFile file, int claimId);
        string GetFilePath(int documentId);
        Stream GetFilestream(string filePath);
        bool DeleteFile(string filePath);
    }
}