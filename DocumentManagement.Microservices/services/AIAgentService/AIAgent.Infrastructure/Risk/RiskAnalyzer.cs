using AIAgent.Application.Abstractions;
using AIAgent.Domain.ValueObjects;

namespace AIAgent.Infrastructure.Risk;

/// <summary>Calculates risk from AI confidence and validation failures.</summary>
public sealed class RiskAnalyzer : IRiskAnalyzer
{
    /// <summary>Produces a simple risk score used by the decision engine.</summary>
    public Task<RiskScore> AnalyzeAsync(DocumentMetadata metadata, MetadataValidationResult validation, CancellationToken cancellationToken)
    {
        var reasons = validation.Errors.ToList();
        if (metadata.ConfidenceScore < 0.80m) reasons.Add("AI confidence is below threshold.");
        var level = reasons.Count == 0 ? "Low" : metadata.ConfidenceScore < 0.60m ? "High" : "Medium";
        return Task.FromResult(new RiskScore(reasons.Count, level, reasons));
    }
}
