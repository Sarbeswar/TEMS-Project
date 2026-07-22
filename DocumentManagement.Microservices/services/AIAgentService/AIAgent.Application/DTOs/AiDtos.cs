namespace AIAgent.Application.DTOs;

/// <summary>Kafka payload consumed from IRMUpload after a document reaches Azure Blob Storage.</summary>
public sealed record DocumentUploadedEvent(Guid DocumentId, string ContainerName, string BlobName, string FileName, string CorrelationId);

/// <summary>Kafka payload published when metadata was extracted and validated.</summary>
public sealed record MetadataExtractedEvent(Guid DocumentId, string DocumentType, string ClientId, string ReferenceNumber, decimal ConfidenceScore, string RiskLevel, string CorrelationId);

/// <summary>Kafka payload published when AI confidence, validation, or risk policy requires human review.</summary>
public sealed record ManualReviewRequiredEvent(Guid DocumentId, string Reason, string CorrelationId);

/// <summary>Response returned to ICMPAPI/dashboard queries.</summary>
public sealed record AiProcessingJobDto(Guid DocumentId, string Status, string? DocumentType, string? RiskLevel, string CorrelationId);
