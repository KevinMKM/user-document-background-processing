using Microsoft.EntityFrameworkCore;
using UserDocumentBackgroundProcessing.Application.Interfaces;
using UserDocumentBackgroundProcessing.Domain.Models;
using UserDocumentBackgroundProcessing.Infrastructure.Data;

namespace UserDocumentBackgroundProcessing.Infrastructure.Repositories
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly ApplicationDbContext _db;
        public OutboxRepository(ApplicationDbContext db) { _db = db; }
        public async Task AddAsync(OutboxMessage message) { await _db.OutboxMessages.AddAsync(message); await _db.SaveChangesAsync(); }
        public async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int limit = 20) => await _db.OutboxMessages.Where(m => !m.Processed).OrderBy(m => m.CreatedAt).Take(limit).ToListAsync();
        public async Task MarkProcessedAsync(OutboxMessage message) { message.Processed = true; message.ProcessedAt = System.DateTime.UtcNow; _db.OutboxMessages.Update(message); await _db.SaveChangesAsync(); }
    }
}
