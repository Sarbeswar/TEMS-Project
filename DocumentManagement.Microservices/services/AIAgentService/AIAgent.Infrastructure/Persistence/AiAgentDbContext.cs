using AIAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIAgent.Infrastructure.Persistence;

/// <summary>EF Core DbContext for SQL Server persistence of AI processing jobs.</summary>
public sealed class AiAgentDbContext : DbContext
{
    public AiAgentDbContext(DbContextOptions<AiAgentDbContext> options) : base(options) { }

    public DbSet<AiDocumentProcessingJob> ProcessingJobs => Set<AiDocumentProcessingJob>();

    /// <summary>Configures aggregate persistence rules and owned value objects.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AiDocumentProcessingJob>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.OwnsOne(x => x.BlobLocation);
            entity.OwnsOne(x => x.Metadata);
            entity.OwnsOne(x => x.RiskScore);
            entity.Ignore(x => x.DomainEvents);
            entity.Property(x => x.CorrelationId).HasMaxLength(100).IsRequired();
        });
    }
}
