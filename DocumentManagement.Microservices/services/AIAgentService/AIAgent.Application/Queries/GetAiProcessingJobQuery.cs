using AIAgent.Application.DTOs;
using MediatR;

namespace AIAgent.Application.Queries;

/// <summary>Query used by ICMPAPI or support dashboards to inspect AI processing status.</summary>
public sealed record GetAiProcessingJobQuery(Guid DocumentId) : IRequest<AiProcessingJobDto?>;
