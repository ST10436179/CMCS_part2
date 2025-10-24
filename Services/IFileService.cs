using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CMCSCopilot.Services
{
    public interface IFileService
    {
        Task<(bool Success, string StoredFileName, string OriginalFileName, long Size, string Error)> SaveFileAsync(IFormFile file);
        void DeleteFile(string storedFileName);
    }
}
