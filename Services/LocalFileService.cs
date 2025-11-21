using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CMCSCopilot.Services
{
    public class LocalFileService : IFileService
    {
        private readonly string _uploadPath;
        private readonly long _maxSize;
        private readonly string[] _allowed;

        public LocalFileService(IConfiguration cfg, IWebHostEnvironment env)
        {
            var cfgSection = cfg.GetSection("FileStorage");
            var uploadRelative = cfgSection.GetValue<string>("UploadPath") ?? "wwwroot/uploads";
            _uploadPath = Path.IsPathRooted(uploadRelative) ? uploadRelative : Path.Combine(env.ContentRootPath, uploadRelative);
            _maxSize = cfgSection.GetValue<long>("MaxFileSizeBytes", 5_242_880);
            _allowed = (cfgSection.GetValue<string>("AllowedExtensions") ?? ".pdf,.docx,.xlsx")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim().ToLowerInvariant()).ToArray();

            if (!Directory.Exists(_uploadPath)) Directory.CreateDirectory(_uploadPath);
        }

        public async Task<(bool Success, string StoredFileName, string OriginalFileName, long Size, string Error)> SaveFileAsync(IFormFile file)
        {
            if (file == null) return (false, null, null, 0, "No file provided");
            if (file.Length == 0) return (false, null, null, 0, "Empty file");
            if (file.Length > _maxSize) return (false, null, null, 0, "File too large");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (Array.IndexOf(_allowed, ext) < 0) return (false, null, null, 0, "Invalid file type");

            var stored = $"{Guid.NewGuid():N}{ext}";
            var path = Path.Combine(_uploadPath, stored);

            try
            {
                using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);
                return (true, stored, file.FileName, file.Length, null);
            }
            catch (Exception ex)
            {
                return (false, null, null, 0, ex.Message);
            }
        }

        public void DeleteFile(string storedFileName)
        {
            if (string.IsNullOrWhiteSpace(storedFileName)) return;
            var path = Path.Combine(_uploadPath, storedFileName);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
