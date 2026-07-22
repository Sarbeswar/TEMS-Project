using AIAgent.Application.Abstractions;
using AIAgent.Application.Commands;
using AIAgent.Application.DTOs;
using AIAgent.Domain.Entities;
using AIAgent.Domain.Repositories;
using AIAgent.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIAgent.Application.Handlers;

/// <summary>
/// Orchestrates OCR, LLM extraction, metadata validation, risk analysis, decisioning, persistence, and Kafka publication.
/// </summary>
public sealed class ProcessDocumentUploadedCommandHandler : IRequestHandler<ProcessDocumentUploadedCommand>
{
    private readonly IBlobStorageClient _blobStorageClient;
    private readonly IOcrService _ocrService;
    private readonly IPromptBuilder _promptBuilder;
    private readonly ILlmService _llmService;
    private readonly IMetadataValidator _metadataValidator;
    private readonly IRiskAnalyzer _riskAnalyzer;
    private readonly IDecisionEngine _decisionEngine;
    private readonly IAiEventPublisher _eventPublisher;
    private readonly IAiDocumentProcessingJobRepository _repository;
    private readonly ILogger<ProcessDocumentUploadedCommandHandler> _logger;

    public ProcessDocumentUploadedCommandHandler(
        IBlobStorageClient blobStorageClient,
        IOcrService ocrService,
        IPromptBuilder promptBuilder,
        ILlmService llmService,
        IMetadataValidator metadataValidator,
        IRiskAnalyzer riskAnalyzer,
        IDecisionEngine decisionEngine,
        IAiEventPublisher eventPublisher,
        IAiDocumentProcessingJobRepository repository,
        ILogger<ProcessDocumentUploadedCommandHandler> logger)
    {
        _blobStorageClient = blobStorageClient;
        _ocrService = ocrService;
        _promptBuilder = promptBuilder;
        _llmService = llmService;
        _metadataValidator = metadataValidator;
        _riskAnalyzer = riskAnalyzer;
        _decisionEngine = decisionEngine;
        _eventPublisher = eventPublisher;
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Processes the uploaded document and publishes either MetadataExtracted or ManualReviewRequired.
    /// </summary>
    public async Task Handle(ProcessDocumentUploadedCommand request, CancellationToken cancellationToken)
    {
        var uploaded = request.UploadedEvent;
        var location = BlobDocumentLocation.Create(uploaded.ContainerName, uploaded.BlobName);
        var job = AiDocumentProcessingJob.Start(uploaded.DocumentId, location, uploaded.CorrelationId);

        await _repository.AddAsync(job, cancellationToken);

        await using var documentStream = await _blobStorageClient.DownloadAsync(location, cancellationToken);
        var text = await _ocrService.ExtractTextAsync(documentStream, cancellationToken);
        var prompt = _promptBuilder.BuildMetadataExtractionPrompt(text);
        var metadata = await _llmService.ExtractMetadataAsync(prompt, cancellationToken);
        var validation = await _metadataValidator.ValidateAsync(metadata, cancellationToken);
        var riskScore = await _riskAnalyzer.AnalyzeAsync(metadata, validation, cancellationToken);
        var decision = _decisionEngine.Decide(metadata, validation, riskScore);

        if (decision.IsSuccessful)
        {
            job.MarkSucceeded(metadata, riskScore);
            await _eventPublisher.PublishMetadataExtractedAsync(
                new MetadataExtractedEvent(uploaded.DocumentId, metadata.DocumentType, metadata.ClientId, metadata.ReferenceNumber, metadata.ConfidenceScore, riskScore.Level, uploaded.CorrelationId),
                cancellationToken);
        }
        else
        {
            job.MarkManualReviewRequired(decision.Reason);
            await _eventPublisher.PublishManualReviewRequiredAsync(new ManualReviewRequiredEvent(uploaded.DocumentId, decision.Reason, uploaded.CorrelationId), cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("AI processing completed for document {DocumentId} with decision {Decision}", uploaded.DocumentId, decision.IsSuccessful);
    }
}
