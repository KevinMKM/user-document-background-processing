namespace UserDocumentBackgroundProcessing.Domain.Models
{
    public enum DocumentStatus { Pending, Processing, Processed, Failed, CleanedUp }

    public class UserDocument
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string? PdfFileName { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public int RetryCount { get; set; } = 0;
        public string? ErrorMessage { get; set; }
        public virtual User? User { get; set; }
    }
}
