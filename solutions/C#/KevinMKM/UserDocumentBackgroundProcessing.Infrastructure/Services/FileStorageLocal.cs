using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserDocumentBackgroundProcessing.Infrastructure.Options;

namespace UserDocumentBackgroundProcessing.Infrastructure.Services
{
    public class FileStorageLocal : IFileStorage
    {
        private readonly string _basePath;
        private readonly ILogger<FileStorageLocal> _logger;
        public string UploadsPath => _basePath;
        public FileStorageLocal(IWebHostEnvironment env, IOptions<FileStorageOptions> opts, ILogger<FileStorageLocal> logger)
        {
            _basePath = Path.Combine(env.ContentRootPath, opts.Value.UploadsFolder ?? "Uploads");
            Directory.CreateDirectory(_basePath);
            _logger = logger;
        }
        public async Task<string> SaveAsync(Stream content, string fileName, Guid userId)
        {
            var userDir = Path.Combine(_basePath, userId.ToString());
            Directory.CreateDirectory(userDir);
            var safe = Path.GetFileName(fileName);
            var name = $"{Guid.NewGuid()}_{safe}";
            var dest = Path.Combine(userDir, name);
            using var fs = new FileStream(dest, FileMode.CreateNew);
            await content.CopyToAsync(fs);
            _logger.LogInformation("Saved file {Dest}", dest);
            return Path.GetRelativePath(_basePath, dest);
        }
        public Task DeleteAsync(string storedFileName)
        {
            if (string.IsNullOrEmpty(storedFileName)) return Task.CompletedTask;
            var path = Path.Combine(_basePath, storedFileName);
            if (File.Exists(path)) File.Delete(path);
            return Task.CompletedTask;
        }
        public Task<string> ConvertToPdfAsync(string storedFileName, Guid documentId)
        {
            var src = Path.Combine(_basePath, storedFileName);
            var pdf = Path.ChangeExtension(src, ".pdf");
            File.Copy(src, pdf, true);
            _logger.LogInformation("Converted {Src} to {Pdf}", src, pdf);
            return Task.FromResult(Path.GetRelativePath(_basePath, pdf));
        }
    }
}
