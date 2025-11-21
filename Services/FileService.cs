using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ContractMonthlyClaimSystem.Models;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Data;

namespace ContractMonthlyClaimSystem.Services
{
    public class FileService : IFileService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly string _uploadFolder;

        public FileService(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
            _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            // Ensure upload directory exists
            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }

        public async Task<Document> UploadFileAsync(IFormFile file, int claimId)
        {
            // Validate file
            var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new Exception($"Invalid file type. Only {string.Join(", ", allowedExtensions)} files are allowed.");
            }

            if (file.Length > 5 * 1024 * 1024) // 5MB limit
            {
                throw new Exception("File size exceeds the 5MB limit.");
            }

            // Generate unique file name
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(_uploadFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create document record
            var document = new Document
            {
                FileName = file.FileName,
                FilePath = $"/uploads/{fileName}",
                UploadDate = DateTime.UtcNow,
                ClaimId = claimId
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return document;
        }

        public string? GetFilePath(int documentId)
        {
            var document = _context.Documents.Find(documentId);
            return document != null ? Path.Combine(_uploadFolder, Path.GetFileName(document.FilePath)) : null;
        }

        public Stream GetFilestream(string filePath)
        {
            var fullPath = Path.Combine(_uploadFolder, Path.GetFileName(filePath));
            return File.OpenRead(fullPath);
        }

        public bool DeleteFile(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_uploadFolder, Path.GetFileName(filePath));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}