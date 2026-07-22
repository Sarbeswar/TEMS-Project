using AIAgent.Application.Abstractions;

namespace AIAgent.Infrastructure.DataLookup;

/// <summary>HTTP client adapter for validating metadata against the existing DataLookup microservice.</summary>
public sealed class DataLookupClient : IDataLookupClient
{
    private readonly HttpClient _httpClient;

    public DataLookupClient(HttpClient httpClient) => _httpClient = httpClient;

    /// <summary>Checks whether the extracted client id exists in enterprise reference data.</summary>
    public async Task<bool> ClientExistsAsync(string clientId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"api/clients/{Uri.EscapeDataString(clientId)}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
