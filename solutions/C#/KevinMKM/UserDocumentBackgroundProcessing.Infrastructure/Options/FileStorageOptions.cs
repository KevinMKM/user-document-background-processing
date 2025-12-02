namespace UserDocumentBackgroundProcessing.Infrastructure.Options
{
    public class FileStorageOptions
    {
        public string? UploadsFolder { get; set; } = "Uploads";
        public int MaxFileSizeMB { get; set; } = 10;
        public string[] AllowedExtensions { get; set; } = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
    }
}
