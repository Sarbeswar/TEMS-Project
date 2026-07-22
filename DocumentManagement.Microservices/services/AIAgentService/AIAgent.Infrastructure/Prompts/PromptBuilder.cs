using AIAgent.Application.Abstractions;

namespace AIAgent.Infrastructure.Prompts;

/// <summary>Builds deterministic prompts that instruct Azure OpenAI to return structured document metadata.</summary>
public sealed class PromptBuilder : IPromptBuilder
{
    /// <summary>Creates the prompt used for document classification and metadata extraction.</summary>
    public string BuildMetadataExtractionPrompt(string extractedText) =>
        $"""
        You are an enterprise document-processing agent.
        Classify the document and extract DocumentType, ClientId, ReferenceNumber, and ConfidenceScore.
        Return JSON only.
        Document text:
        {extractedText}
        """;
}
