using AIAgent.Application.DTOs;
using AIAgent.Domain.ValueObjects;

namespace AIAgent.Application.Abstractions;

/// <summary>Downloads uploaded files from Azure Blob Storage for AI processing.</summary>
public interface IBlobStorageClient { Task<Stream> DownloadAsync(BlobDocumentLocation location, CancellationToken cancellationToken); }

/// <summary>Runs OCR and returns raw text from scanned or digital documents.</summary>
public interface IOcrService { Task<string> ExtractTextAsync(Stream documentStream, CancellationToken cancellationToken); }

/// <summary>Builds consistent prompts so LLM output remains structured and auditable.</summary>
public interface IPromptBuilder { string BuildMetadataExtractionPrompt(string extractedText); }

/// <summary>Calls Azure OpenAI and converts extracted text into structured metadata.</summary>
public interface ILlmService { Task<DocumentMetadata> ExtractMetadataAsync(string prompt, CancellationToken cancellationToken); }

/// <summary>Validates AI metadata against existing enterprise reference data.</summary>
public interface IMetadataValidator { Task<MetadataValidationResult> ValidateAsync(DocumentMetadata metadata, CancellationToken cancellationToken); }

/// <summary>Computes business risk based on extracted metadata and validation results.</summary>
public interface IRiskAnalyzer { Task<RiskScore> AnalyzeAsync(DocumentMetadata metadata, MetadataValidationResult validation, CancellationToken cancellationToken); }

/// <summary>Applies final business rules to determine auto-processing versus manual review.</summary>
public interface IDecisionEngine { AiDecision Decide(DocumentMetadata metadata, MetadataValidationResult validation, RiskScore riskScore); }

/// <summary>Reads authoritative lookup data from the existing DataLookup microservice.</summary>
public interface IDataLookupClient { Task<bool> ClientExistsAsync(string clientId, CancellationToken cancellationToken); }

/// <summary>Publishes integration events to Kafka topics consumed by Replication and Notification.</summary>
public interface IAiEventPublisher
{
    Task PublishMetadataExtractedAsync(MetadataExtractedEvent integrationEvent, CancellationToken cancellationToken);
    Task PublishManualReviewRequiredAsync(ManualReviewRequiredEvent integrationEvent, CancellationToken cancellationToken);
}

/// <summary>Validation result used by the decision engine and risk analyzer.</summary>
public sealed record MetadataValidationResult(bool IsValid, IReadOnlyCollection<string> Errors);

/// <summary>Decision result telling the handler which Kafka event to publish.</summary>
public sealed record AiDecision(bool IsSuccessful, string Reason);
