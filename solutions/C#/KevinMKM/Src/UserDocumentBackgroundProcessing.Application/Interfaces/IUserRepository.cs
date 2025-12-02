using UserDocumentBackgroundProcessing.Domain.Models;

namespace UserDocumentBackgroundProcessing.Application.Interfaces
{
    public interface IUserRepository
    {
        Task AddUserAsync(User user);
        Task<User?> GetUserAsync(Guid id);
        Task AddDocumentAsync(UserDocument doc);
        Task<UserDocument?> GetDocumentAsync(Guid id);
        Task SaveChangesAsync();
    }
}
