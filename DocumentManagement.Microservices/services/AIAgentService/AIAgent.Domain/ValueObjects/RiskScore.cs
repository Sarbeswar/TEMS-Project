namespace AIAgent.Domain.ValueObjects;

/// <summary>
/// Value object that describes the business risk outcome for an extracted document.
/// </summary>
public sealed record RiskScore(decimal Score, string Level, IReadOnlyCollection<string> Reasons);
