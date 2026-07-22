using AIAgent.Application.Abstractions;
using AIAgent.Domain.ValueObjects;

namespace AIAgent.Infrastructure.Decision;

/// <summary>Centralizes auto-processing versus manual-review business policy.</summary>
public sealed class DecisionEngine : IDecisionEngine
{
    /// <summary>Approves only validated low-risk metadata with sufficient AI confidence.</summary>
    public AiDecision Decide(DocumentMetadata metadata, MetadataValidationResult validation, RiskScore riskScore)
    {
        if (!validation.IsValid) return new AiDecision(false, string.Join("; ", validation.Errors));
        if (metadata.ConfidenceScore < 0.80m) return new AiDecision(false, "AI confidence is below automatic-processing threshold.");
        if (riskScore.Level.Equals("High", StringComparison.OrdinalIgnoreCase)) return new AiDecision(false, "High risk document requires manual review.");
        return new AiDecision(true, "Metadata extracted and validated successfully.");
    }
}
