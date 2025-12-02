namespace UserDocumentBackgroundProcessing.Infrastructure.Services
{
    public interface IEmailSender 
    { 
      Task SendAsync(string to, string subject, string body); 
      Task<bool> TestConnectionAsync(); 
    }
}
