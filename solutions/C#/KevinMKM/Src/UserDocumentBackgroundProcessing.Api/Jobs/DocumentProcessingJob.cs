using Hangfire;
using UserDocumentBackgroundProcessing.Application.Interfaces;
using UserDocumentBackgroundProcessing.Domain.Models;
using UserDocumentBackgroundProcessing.Infrastructure.Services;

namespace UserDocumentBackgroundProcessing.Api.Jobs
{
    public class DocumentProcessingJob
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<DocumentProcessingJob> _logger;
        public DocumentProcessingJob(IServiceProvider provider, ILogger<DocumentProcessingJob> logger) { _provider = provider; _logger = logger; }

        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 300, 600 })]
        public async Task Execute(Guid documentId)
        {
            using var scope = _provider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var outbox = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorage>();

            var doc = await repo.GetDocumentAsync(documentId);
            if (doc == null) { _logger.LogWarning("Document not found {Id}", documentId); return; }

            try
            {
                doc.Status = DocumentStatus.Processing;
                await repo.SaveChangesAsync();

                var pdfRel = await fileStorage.ConvertToPdfAsync(doc.StoredFileName, documentId);
                doc.PdfFileName = pdfRel;
                doc.Status = DocumentStatus.Processed;
                doc.ProcessedAt = DateTime.UtcNow;
                await repo.SaveChangesAsync();

                var payload = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string,string>{{"to",doc.User?.Email ?? string.Empty},{"subject","Document processed"},{"body",$"Your document {doc.OriginalFileName} has been processed."}});
                await outbox.AddAsync(new OutboxMessage{ Type = "Completion", Payload = payload });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Processing failed for {Id}", documentId);
                doc.RetryCount++;
                doc.ErrorMessage = ex.Message;
                doc.Status = doc.RetryCount >= 3 ? DocumentStatus.Failed : DocumentStatus.Pending;
                await repo.SaveChangesAsync();
                throw;
            }
        }
    }
}
