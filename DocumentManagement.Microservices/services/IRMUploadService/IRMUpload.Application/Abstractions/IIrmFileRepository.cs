using IRMUpload.Domain.Entities;

namespace IRMUpload.Application.Abstractions;

public interface IIrmFileRepository
{
    Task<IrmFileAggregate?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken);
    Task SaveAsync(IrmFileAggregate aggregate, string correlationId, CancellationToken cancellationToken);
}
