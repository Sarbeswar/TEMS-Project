using IRMUpload.Application.Abstractions;

namespace IRMUpload.Application.Commands;

public record CreateIrmFileCommand(Guid FileId, string FileName, string StoragePath, string RequestedBy, string CorrelationId);
public record UpdateIrmFileCommand(Guid FileId, string FileName, string RequestedBy, string CorrelationId);
public record DeleteIrmFileCommand(Guid FileId, string RequestedBy, string CorrelationId);

public class IrmFileCommandHandlers
{
    private readonly IIrmFileRepository _repository;

    public IrmFileCommandHandlers(IIrmFileRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(CreateIrmFileCommand command, CancellationToken cancellationToken)
    {
        var aggregate = IRMUpload.Domain.Entities.IrmFileAggregate.Create(command.FileId, command.FileName, command.StoragePath, command.RequestedBy);
        await _repository.SaveAsync(aggregate, command.CorrelationId, cancellationToken);
    }

    public async Task Handle(UpdateIrmFileCommand command, CancellationToken cancellationToken)
    {
        var aggregate = await _repository.GetByIdAsync(command.FileId, cancellationToken)
                        ?? throw new InvalidOperationException($"File {command.FileId} not found.");

        aggregate.Rename(command.FileName, command.RequestedBy);
        await _repository.SaveAsync(aggregate, command.CorrelationId, cancellationToken);
    }

    public async Task Handle(DeleteIrmFileCommand command, CancellationToken cancellationToken)
    {
        var aggregate = await _repository.GetByIdAsync(command.FileId, cancellationToken)
                        ?? throw new InvalidOperationException($"File {command.FileId} not found.");

        aggregate.Delete(command.RequestedBy);
        await _repository.SaveAsync(aggregate, command.CorrelationId, cancellationToken);
    }
}
