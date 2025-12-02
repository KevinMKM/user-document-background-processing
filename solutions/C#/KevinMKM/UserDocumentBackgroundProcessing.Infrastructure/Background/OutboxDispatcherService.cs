using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserDocumentBackgroundProcessing.Application.Interfaces;
using UserDocumentBackgroundProcessing.Infrastructure.Services;

namespace UserDocumentBackgroundProcessing.Infrastructure.Background
{
    public class OutboxDispatcherService : BackgroundService
    {
        private readonly ILogger<OutboxDispatcherService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

        public OutboxDispatcherService(IServiceScopeFactory scopeFactory, ILogger<OutboxDispatcherService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox dispatcher started");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                    var emailSender = scope.ServiceProvider.GetRequiredService<EmailSenderResilient>();
                    var msgs = await outboxRepo.GetUnprocessedMessagesAsync(20);
                    foreach (var m in msgs)
                    {
                        try
                        {
                            var doc = JsonSerializer.Deserialize<Dictionary<string,string>>(m.Payload) ?? new Dictionary<string,string>();
                            var to = doc.GetValueOrDefault("to") ?? string.Empty;
                            var subject = doc.GetValueOrDefault("subject") ?? m.Type;
                            var body = doc.GetValueOrDefault("body") ?? string.Empty;
                            await emailSender.SendAsync(to, subject, body);
                            await outboxRepo.MarkProcessedAsync(m);
                            _logger.LogInformation("Outbox message {Id} processed", m.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Outbox message {Id} failed", m.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Outbox dispatcher cycle failed");
                }
                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
