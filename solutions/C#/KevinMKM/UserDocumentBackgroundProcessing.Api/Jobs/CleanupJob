using Hangfire;
using UserDocumentBackgroundProcessing.Domain.Models;
using UserDocumentBackgroundProcessing.Infrastructure.Data;
using UserDocumentBackgroundProcessing.Infrastructure.Services;

namespace UserDocumentBackgroundProcessing.Api.Jobs
{
    public class CleanupJob
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<CleanupJob> _logger;
        public CleanupJob(IServiceProvider provider, ILogger<CleanupJob> logger) { _provider = provider; _logger = logger; }

        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 300, 600 })]
        public async Task Execute()
        {
            using var scope = _provider.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
            _logger.LogInformation("Cleanup job started");
            var cutoff = DateTime.UtcNow.AddDays(-7);
            var toClean = ctx.UserDocuments.Where(d => (d.Status == DocumentStatus.Pending || d.Status == DocumentStatus.Failed) && d.UploadedAt < cutoff).ToList();
            foreach (var d in toClean)
            {
                try
                {
                    await fileStorage.DeleteAsync(d.StoredFileName);
                    if (!string.IsNullOrEmpty(d.PdfFileName)) await fileStorage.DeleteAsync(d.PdfFileName);
                    d.Status = DocumentStatus.CleanedUp;
                    ctx.UserDocuments.Update(d);
                }
                catch (Exception ex) { _logger.LogError(ex, "Cleanup error for {Id}", d.Id); }
            }
            await ctx.SaveChangesAsync();
            _logger.LogInformation("Cleanup job finished");
        }
    }
}
