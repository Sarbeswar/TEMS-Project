using AIAgent.Application.Abstractions;
using AIAgent.Domain.ValueObjects;

namespace AIAgent.Infrastructure.LLM;

/// <summary>Azure OpenAI adapter that transforms OCR text into normalized metadata.</summary>
public sealed class AzureOpenAiLlmService : ILlmService
{
    /// <summary>Calls the LLM using the supplied prompt and returns structured metadata for validation.</summary>
    public Task<DocumentMetadata> ExtractMetadataAsync(string prompt, CancellationToken cancellationToken)
    {
        // Production code should call Azure OpenAI Chat Completions and deserialize strict JSON output.
        var metadata = new DocumentMetadata("Invoice", "CLIENT-001", "REF-001", 0.92m);
        return Task.FromResult(metadata);
    }
}
