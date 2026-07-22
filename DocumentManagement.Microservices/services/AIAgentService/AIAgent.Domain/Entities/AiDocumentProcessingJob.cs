using AIAgent.Domain.Events;
using AIAgent.Domain.ValueObjects;

namespace AIAgent.Domain.Entities;

/// <summary>
/// Aggregate root that represents the AI processing lifecycle for one uploaded enterprise document.
/// It exists so OCR, LLM extraction, validation, risk scoring, and final decisions are tracked consistently.
/// </summary>
public sealed class AiDocumentProcessingJob
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>Database identity for the processing job.</summary>
    public Guid Id { get; private set; }

    /// <summary>Original document id emitted by IRMUpload through Kafka.</summary>
    public Guid DocumentId { get; private set; }

    /// <summary>Azure Blob path from which AIAgentService downloads the uploaded file.</summary>
    public BlobDocumentLocation BlobLocation { get; private set; } = null!;

    /// <summary>Current business state for dashboard, retry, and manual-review decisions.</summary>
    public ProcessingStatus Status { get; private set; }

    /// <summary>Extracted metadata after OCR and Azure OpenAI analysis.</summary>
    public DocumentMetadata? Metadata { get; private set; }

    /// <summary>Risk score produced by the risk analyzer.</summary>
    public RiskScore? RiskScore { get; private set; }

    /// <summary>Correlation id propagated across gateway, ICMPAPI, Kafka, and this service.</summary>
    public string CorrelationId { get; private set; } = string.Empty;

    /// <summary>UTC timestamp for auditability.</summary>
    public DateTime CreatedOnUtc { get; private set; }

    /// <summary>UTC timestamp for update tracking.</summary>
    public DateTime? CompletedOnUtc { get; private set; }

    /// <summary>Domain events waiting to be persisted and/or published.</summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private AiDocumentProcessingJob() { }

    /// <summary>
    /// Starts a new AI processing job when DocumentUploaded is consumed from Kafka.
    /// </summary>
    public static AiDocumentProcessingJob Start(Guid documentId, BlobDocumentLocation blobLocation, string correlationId)
    {
        var job = new AiDocumentProcessingJob
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            BlobLocation = blobLocation,
            Status = ProcessingStatus.Started,
            CorrelationId = correlationId,
            CreatedOnUtc = DateTime.UtcNow
        };

        job.AddDomainEvent(new AiProcessingStartedDomainEvent(job.Id, documentId, correlationId, DateTime.UtcNow));
        return job;
    }

    /// <summary>
    /// Completes processing successfully after metadata validation and risk decision succeed.
    /// </summary>
    public void MarkSucceeded(DocumentMetadata metadata, RiskScore riskScore)
    {
        Metadata = metadata;
        RiskScore = riskScore;
        Status = ProcessingStatus.Succeeded;
        CompletedOnUtc = DateTime.UtcNow;
        AddDomainEvent(new AiProcessingSucceededDomainEvent(Id, DocumentId, CorrelationId, DateTime.UtcNow));
    }

    /// <summary>
    /// Sends the document to manual review when AI confidence, validation, or risk policy fails.
    /// </summary>
    public void MarkManualReviewRequired(string reason)
    {
        Status = ProcessingStatus.ManualReviewRequired;
        CompletedOnUtc = DateTime.UtcNow;
        AddDomainEvent(new ManualReviewRequiredDomainEvent(Id, DocumentId, reason, CorrelationId, DateTime.UtcNow));
    }

    /// <summary>Clears events after they are persisted/published to prevent duplicate dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}

/// <summary>Business statuses used by dashboard, retry workers, and support teams.</summary>
public enum ProcessingStatus
{
    Started = 1,
    Succeeded = 2,
    ManualReviewRequired = 3,
    Failed = 4
}
