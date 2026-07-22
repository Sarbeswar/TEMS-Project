namespace AIAgent.Domain.Events;

/// <summary>Marker interface for domain events raised inside the AI Agent bounded context.</summary>
public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}

/// <summary>Raised when AIAgentService starts processing a Kafka DocumentUploaded event.</summary>
public sealed record AiProcessingStartedDomainEvent(Guid JobId, Guid DocumentId, string CorrelationId, DateTime OccurredOnUtc) : IDomainEvent;

/// <summary>Raised when metadata extraction, validation, and risk analysis complete successfully.</summary>
public sealed record AiProcessingSucceededDomainEvent(Guid JobId, Guid DocumentId, string CorrelationId, DateTime OccurredOnUtc) : IDomainEvent;

/// <summary>Raised when the document must be sent to a human reviewer.</summary>
public sealed record ManualReviewRequiredDomainEvent(Guid JobId, Guid DocumentId, string Reason, string CorrelationId, DateTime OccurredOnUtc) : IDomainEvent;
