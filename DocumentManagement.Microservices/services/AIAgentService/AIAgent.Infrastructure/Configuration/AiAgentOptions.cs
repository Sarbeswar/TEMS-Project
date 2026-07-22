namespace AIAgent.Infrastructure.Configuration;

/// <summary>Strongly typed settings for external dependencies used by AIAgentService.</summary>
public sealed class AiAgentOptions
{
    public string SqlConnectionString { get; init; } = string.Empty;
    public string BlobConnectionString { get; init; } = string.Empty;
    public string AzureOpenAiEndpoint { get; init; } = string.Empty;
    public string AzureOpenAiDeployment { get; init; } = string.Empty;
    public string KafkaBootstrapServers { get; init; } = string.Empty;
    public string DataLookupBaseUrl { get; init; } = string.Empty;
}
