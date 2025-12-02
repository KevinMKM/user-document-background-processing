using Microsoft.EntityFrameworkCore;
using UserDocumentBackgroundProcessing.Application.Interfaces;
using UserDocumentBackgroundProcessing.Domain.Models;
using UserDocumentBackgroundProcessing.Infrastructure.Data;

namespace UserDocumentBackgroundProcessing.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        public UserRepository(ApplicationDbContext db) { _db = db; }
        public async Task AddUserAsync(User user) { await _db.Users.AddAsync(user); }
        public async Task<User?> GetUserAsync(Guid id) => await _db.Users.Include(u => u.Document).FirstOrDefaultAsync(u => u.Id == id);
        public async Task AddDocumentAsync(UserDocument doc) { await _db.UserDocuments.AddAsync(doc); }
        public async Task<UserDocument?> GetDocumentAsync(Guid id) => await _db.UserDocuments.FirstOrDefaultAsync(d => d.Id == id);
        public Task SaveChangesAsync() => _db.SaveChangesAsync();
    }
}
