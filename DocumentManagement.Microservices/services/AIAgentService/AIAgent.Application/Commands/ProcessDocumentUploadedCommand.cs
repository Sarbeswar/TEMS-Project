using AIAgent.Application.DTOs;
using MediatR;

namespace AIAgent.Application.Commands;

/// <summary>
/// Command triggered by the Kafka consumer to process a newly uploaded document.
/// </summary>
public sealed record ProcessDocumentUploadedCommand(DocumentUploadedEvent UploadedEvent) : IRequest;
