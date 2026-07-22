namespace AIAgent.Domain.ValueObjects;

/// <summary>
/// Value object holding normalized metadata extracted by OCR and Azure OpenAI.
/// </summary>
public sealed record DocumentMetadata(string DocumentType, string ClientId, string ReferenceNumber, decimal ConfidenceScore);
