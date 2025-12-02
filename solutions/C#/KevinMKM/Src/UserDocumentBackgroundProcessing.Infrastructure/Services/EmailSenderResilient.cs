using Microsoft.Extensions.Logging;
using Polly;

namespace UserDocumentBackgroundProcessing.Infrastructure.Services
{
    public class EmailSenderResilient : IEmailSender
    {
        private readonly ILogger<EmailSenderResilient> _logger;
        private readonly AsyncPolicy _policy;
        public EmailSenderResilient(ILogger<EmailSenderResilient> logger)
        {
            _logger = logger;
            _policy = Policy.Handle<Exception>().WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(2 * i),
                (ex, ts) => _logger.LogWarning(ex, "Email send retry after {Delay}", ts));
        }
        public async Task SendAsync(string to, string subject, string body)
        {
            await _policy.ExecuteAsync(async () =>
            {
                await Task.Delay(100);
                _logger.LogInformation("Simulated email send to {To} subject {S}", to, subject);
                Console.WriteLine($"[EMAIL] To:{to} Subject:{subject} Body:{body}");
            });
        }
        public Task<bool> TestConnectionAsync() => Task.FromResult(true);
    }
}
