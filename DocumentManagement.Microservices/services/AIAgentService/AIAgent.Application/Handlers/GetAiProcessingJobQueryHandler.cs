using AIAgent.Application.DTOs;
using AIAgent.Application.Queries;
using AIAgent.Domain.Repositories;
using MediatR;

namespace AIAgent.Application.Handlers;

/// <summary>Reads a processing job without invoking OCR/LLM work, keeping read path separate from command path.</summary>
public sealed class GetAiProcessingJobQueryHandler : IRequestHandler<GetAiProcessingJobQuery, AiProcessingJobDto?>
{
    private readonly IAiDocumentProcessingJobRepository _repository;

    public GetAiProcessingJobQueryHandler(IAiDocumentProcessingJobRepository repository) => _repository = repository;

    /// <summary>Returns current AI job status for dashboard and troubleshooting screens.</summary>
    public async Task<AiProcessingJobDto?> Handle(GetAiProcessingJobQuery request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetByDocumentIdAsync(request.DocumentId, cancellationToken);
        return job is null
            ? null
            : new AiProcessingJobDto(job.DocumentId, job.Status.ToString(), job.Metadata?.DocumentType, job.RiskScore?.Level, job.CorrelationId);
    }
}
