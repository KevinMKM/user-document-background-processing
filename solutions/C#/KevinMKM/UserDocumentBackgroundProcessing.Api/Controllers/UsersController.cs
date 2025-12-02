using System.Text.Json;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UserDocumentBackgroundProcessing.Api.Jobs;
using UserDocumentBackgroundProcessing.Application.Interfaces;
using UserDocumentBackgroundProcessing.Domain.Models;
using UserDocumentBackgroundProcessing.Infrastructure.Options;
using UserDocumentBackgroundProcessing.Infrastructure.Services;

namespace UserDocumentBackgroundProcessing.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repo;
        private readonly IOutboxRepository _outbox;
        private readonly IFileStorage _fileStorage;
        private readonly IBackgroundJobClient _jobs;
        private readonly FileStorageOptions _fsOptions;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserRepository repo, IOutboxRepository outbox, IFileStorage fileStorage, IBackgroundJobClient jobs, IOptions<FileStorageOptions> fileOptions, ILogger<UsersController> logger)
        {
            _repo = repo; _outbox = outbox; _fileStorage = fileStorage; _jobs = jobs; _fsOptions = fileOptions.Value; _logger = logger;
        }

        [HttpPost("register")]
        [RequestSizeLimit(30 * 1024 * 1024)]
        public async Task<IActionResult> Register([FromForm] string name, [FromForm] string email, [FromForm] IFormFile document)
        {
            if (document == null || document.Length == 0) return BadRequest("Document required");
            var ext = Path.GetExtension(document.FileName).ToLowerInvariant();
            if (!_fsOptions.AllowedExtensions.Contains(ext)) return BadRequest("File type not allowed");
            if (document.Length > _fsOptions.MaxFileSizeMB * 1024L * 1024L) return BadRequest($"Max file size {_fsOptions.MaxFileSizeMB}MB");

            var user = new User { Name = name, Email = email };
            await _repo.AddUserAsync(user);

            // Save file and create document record
            var storedRelative = await _fileStorage.SaveAsync(document.OpenReadStream(), document.FileName, user.Id);
            var doc = new UserDocument { UserId = user.Id, OriginalFileName = document.FileName, StoredFileName = storedRelative, Status = DocumentStatus.Pending };
            await _repo.AddDocumentAsync(doc);

            // Create outbox welcome message
            var payload = JsonSerializer.Serialize(new Dictionary<string,string>{{"to", user.Email},{"subject","Welcome"},{"body",$"Welcome {user.Name}"}});
            var outMsg = new OutboxMessage { Type = "Welcome", Payload = payload };
            await _outbox.AddAsync(outMsg);

            await _repo.SaveChangesAsync();

            // Schedule processing after 30s
            _jobs.Schedule<DocumentProcessingJob>(j => j.Execute(doc.Id), TimeSpan.FromSeconds(30));

            return Created(string.Empty, new { userId = user.Id, documentId = doc.Id, status = "Registered" });
        }

        [HttpGet("documents/{id}/status")]
        public async Task<IActionResult> GetStatus(Guid id)
        {
            var doc = await _repo.GetDocumentAsync(id);
            if (doc == null) return NotFound();
            return Ok(new { doc.Id, status = doc.Status.ToString(), doc.RetryCount, doc.ErrorMessage, doc.UploadedAt, doc.ProcessedAt });
        }
    }
}
