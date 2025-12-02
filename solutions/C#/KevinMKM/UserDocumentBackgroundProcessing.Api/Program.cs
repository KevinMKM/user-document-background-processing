using Hangfire;
using Hangfire.Common;
using Hangfire.MemoryStorage;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UserDocumentBackgroundProcessing.Api.Jobs;
using UserDocumentBackgroundProcessing.Application.Interfaces;
using UserDocumentBackgroundProcessing.Infrastructure.Background;
using UserDocumentBackgroundProcessing.Infrastructure.Data;
using UserDocumentBackgroundProcessing.Infrastructure.Options;
using UserDocumentBackgroundProcessing.Infrastructure.Repositories;
using UserDocumentBackgroundProcessing.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).WriteTo.Console().CreateLogger();
builder.Host.UseSerilog();

// Configuration
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));

// DbContext - SQL Server if connection string provided, otherwise InMemory
var cs = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(cs))
    builder.Services.AddDbContext<ApplicationDbContext>(opts => opts.UseSqlServer(cs));
else
    builder.Services.AddDbContext<ApplicationDbContext>(opts => opts.UseInMemoryDatabase("UserDocsDB"));

// Repositories and infra services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<IFileStorage, FileStorageLocal>();
builder.Services.AddScoped<EmailSenderResilient>();

// Outbox dispatcher background service
builder.Services.AddHostedService<OutboxDispatcherService>();

// Hangfire - use SQL storage if connection string, else Memory
if (!string.IsNullOrEmpty(cs))
{
    builder.Services.AddHangfire(cfg => cfg.UseSqlServerStorage(cs));
}
else
{
    builder.Services.AddHangfire(cfg => cfg.UseMemoryStorage());
}
builder.Services.AddHangfireServer();

// Jobs
builder.Services.AddScoped<DocumentProcessingJob>();
builder.Services.AddScoped<CleanupJob>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard("/hangfire");
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// schedule recurring cleanup at midnight Europe/Berlin
using (var scope = app.Services.CreateScope())
{
    var recurring = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
    recurring.AddOrUpdate("nightly-cleanup", Job.FromExpression<CleanupJob>(x => x.Execute()), "0 0 * * *", tz);
    Log.Information("Recurring cleanup scheduled (Europe/Berlin 00:00)");
}

app.Run();
