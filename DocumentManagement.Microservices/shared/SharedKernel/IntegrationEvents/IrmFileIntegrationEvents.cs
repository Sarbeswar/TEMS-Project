namespace SharedKernel.IntegrationEvents;

public abstract record IntegrationEvent(Guid EventId, DateTime OccurredOnUtc, string CorrelationId);

public record IrmFileCreatedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOnUtc,
    string CorrelationId,
    Guid FileId,
    string FileName,
    string StoragePath,
    string RequestedBy) : IntegrationEvent(EventId, OccurredOnUtc, CorrelationId);

public record IrmFileUpdatedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOnUtc,
    string CorrelationId,
    Guid FileId,
    string FileName,
    string RequestedBy) : IntegrationEvent(EventId, OccurredOnUtc, CorrelationId);

public record IrmFileDeletedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOnUtc,
    string CorrelationId,
    Guid FileId,
    string RequestedBy) : IntegrationEvent(EventId, OccurredOnUtc, CorrelationId);
