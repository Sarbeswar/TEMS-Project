using AIAgent.Domain.Entities;
using AIAgent.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AIAgent.Infrastructure.Persistence;

/// <summary>SQL Server repository implementation for AI processing jobs.</summary>
public sealed class AiDocumentProcessingJobRepository : IAiDocumentProcessingJobRepository
{
    private readonly AiAgentDbContext _dbContext;

    public AiDocumentProcessingJobRepository(AiAgentDbContext dbContext) => _dbContext = dbContext;

    /// <summary>Stages a new processing job for persistence.</summary>
    public async Task AddAsync(AiDocumentProcessingJob job, CancellationToken cancellationToken) => await _dbContext.ProcessingJobs.AddAsync(job, cancellationToken);

    /// <summary>Loads a job by document id for query APIs or idempotent message handling.</summary>
    public async Task<AiDocumentProcessingJob?> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken) =>
        await _dbContext.ProcessingJobs.FirstOrDefaultAsync(x => x.DocumentId == documentId, cancellationToken);

    /// <summary>Commits changes using EF Core async APIs to avoid blocking request threads.</summary>
    public async Task SaveChangesAsync(CancellationToken cancellationToken) => await _dbContext.SaveChangesAsync(cancellationToken);
}
