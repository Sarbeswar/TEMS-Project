using AIAgent.Domain.Entities;

namespace AIAgent.Domain.Repositories;

/// <summary>
/// Repository abstraction for persisting AI processing jobs without coupling the domain to SQL Server.
/// </summary>
public interface IAiDocumentProcessingJobRepository
{
    Task AddAsync(AiDocumentProcessingJob job, CancellationToken cancellationToken);
    Task<AiDocumentProcessingJob?> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
