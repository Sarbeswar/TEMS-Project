using IRMUpload.Application.Abstractions;

namespace IRMUpload.Application.Queries;

public record GetIrmFileByIdQuery(Guid FileId);
public record IrmFileReadModel(Guid FileId, string FileName, string StoragePath, bool IsDeleted);

public class GetIrmFileByIdQueryHandler
{
    private readonly IIrmFileRepository _repository;

    public GetIrmFileByIdQueryHandler(IIrmFileRepository repository)
    {
        _repository = repository;
    }

    public async Task<IrmFileReadModel?> Handle(GetIrmFileByIdQuery query, CancellationToken cancellationToken)
    {
        var aggregate = await _repository.GetByIdAsync(query.FileId, cancellationToken);
        if (aggregate is null)
        {
            return null;
        }

        return new IrmFileReadModel(aggregate.Id, aggregate.FileName, aggregate.StoragePath, aggregate.IsDeleted);
    }
}
