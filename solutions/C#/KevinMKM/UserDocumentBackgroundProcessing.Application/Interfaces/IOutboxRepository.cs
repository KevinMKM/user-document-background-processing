using UserDocumentBackgroundProcessing.Domain.Models;

namespace UserDocumentBackgroundProcessing.Application.Interfaces
{
    public interface IOutboxRepository
    {
        Task AddAsync(OutboxMessage message);
        Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int limit = 20);
        Task MarkProcessedAsync(OutboxMessage message);
    }
}
