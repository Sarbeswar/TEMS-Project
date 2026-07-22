using AIAgent.Application.Abstractions;

namespace AIAgent.Infrastructure.OCR;

/// <summary>OCR adapter; replace placeholder logic with Azure AI Document Intelligence in production.</summary>
public sealed class AzureOcrService : IOcrService
{
    /// <summary>Extracts raw text from the document stream for downstream LLM metadata extraction.</summary>
    public async Task<string> ExtractTextAsync(Stream documentStream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(documentStream);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
