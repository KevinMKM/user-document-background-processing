namespace UserDocumentBackgroundProcessing.Infrastructure.Services
{
    public interface IFileStorage
    {
        string UploadsPath { get; }
        Task<string> SaveAsync(Stream content, string fileName, Guid userId);
        Task<string> ConvertToPdfAsync(string storedFileName, Guid documentId);
        Task DeleteAsync(string storedFileName);
    }
}
