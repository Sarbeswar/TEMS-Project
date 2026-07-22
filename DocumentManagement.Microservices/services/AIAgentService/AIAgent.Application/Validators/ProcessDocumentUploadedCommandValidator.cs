using AIAgent.Application.Commands;
using FluentValidation;

namespace AIAgent.Application.Validators;

/// <summary>Validates Kafka command data before costly OCR/LLM processing begins.</summary>
public sealed class ProcessDocumentUploadedCommandValidator : AbstractValidator<ProcessDocumentUploadedCommand>
{
    public ProcessDocumentUploadedCommandValidator()
    {
        RuleFor(x => x.UploadedEvent.DocumentId).NotEmpty();
        RuleFor(x => x.UploadedEvent.ContainerName).NotEmpty();
        RuleFor(x => x.UploadedEvent.BlobName).NotEmpty();
        RuleFor(x => x.UploadedEvent.CorrelationId).NotEmpty();
    }
}
