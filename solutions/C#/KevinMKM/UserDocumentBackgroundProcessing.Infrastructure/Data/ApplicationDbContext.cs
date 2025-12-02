using Microsoft.EntityFrameworkCore;
using UserDocumentBackgroundProcessing.Domain.Models;

namespace UserDocumentBackgroundProcessing.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }
        public DbSet<User> Users => Set<User>();
        public DbSet<UserDocument> UserDocuments => Set<UserDocument>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasOne(u => u.Document).WithOne(d => d.User).HasForeignKey<UserDocument>(d => d.UserId);
            base.OnModelCreating(modelBuilder);
        }
    }
}
