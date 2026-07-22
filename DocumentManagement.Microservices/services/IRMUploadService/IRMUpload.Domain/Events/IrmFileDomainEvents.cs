namespace IRMUpload.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}

public record IrmFileCreated(Guid FileId, string FileName, string StoragePath, string RequestedBy, DateTime OccurredOnUtc) : IDomainEvent;
public record IrmFileUpdated(Guid FileId, string FileName, string RequestedBy, DateTime OccurredOnUtc) : IDomainEvent;
public record IrmFileDeleted(Guid FileId, string RequestedBy, DateTime OccurredOnUtc) : IDomainEvent;
