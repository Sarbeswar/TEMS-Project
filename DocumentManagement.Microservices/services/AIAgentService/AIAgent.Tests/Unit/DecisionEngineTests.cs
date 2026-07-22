using AIAgent.Application.Abstractions;
using AIAgent.Domain.ValueObjects;
using AIAgent.Infrastructure.Decision;
using Xunit;

namespace AIAgent.Tests.Unit;

/// <summary>Unit tests document the most important decision-engine business rules.</summary>
public sealed class DecisionEngineTests
{
    [Fact]
    public void Decide_ReturnsSuccess_WhenMetadataIsValidAndLowRisk()
    {
        var engine = new DecisionEngine();
        var metadata = new DocumentMetadata("Invoice", "CLIENT-001", "REF-001", 0.95m);
        var validation = new MetadataValidationResult(true, Array.Empty<string>());
        var risk = new RiskScore(0, "Low", Array.Empty<string>());

        var decision = engine.Decide(metadata, validation, risk);

        Assert.True(decision.IsSuccessful);
    }

    [Fact]
    public void Decide_ReturnsManualReview_WhenConfidenceIsLow()
    {
        var engine = new DecisionEngine();
        var metadata = new DocumentMetadata("Invoice", "CLIENT-001", "REF-001", 0.50m);
        var validation = new MetadataValidationResult(true, Array.Empty<string>());
        var risk = new RiskScore(1, "Medium", new[] { "AI confidence is below threshold." });

        var decision = engine.Decide(metadata, validation, risk);

        Assert.False(decision.IsSuccessful);
    }
}
