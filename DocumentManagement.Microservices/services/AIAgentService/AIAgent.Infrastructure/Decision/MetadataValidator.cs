using AIAgent.Application.Abstractions;
using AIAgent.Domain.ValueObjects;

namespace AIAgent.Infrastructure.Decision;

/// <summary>Validates extracted metadata using local rules and DataLookup reference checks.</summary>
public sealed class MetadataValidator : IMetadataValidator
{
    private readonly IDataLookupClient _dataLookupClient;

    public MetadataValidator(IDataLookupClient dataLookupClient) => _dataLookupClient = dataLookupClient;

    /// <summary>Ensures mandatory metadata exists and client id is recognized by DataLookup.</summary>
    public async Task<MetadataValidationResult> ValidateAsync(DocumentMetadata metadata, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(metadata.DocumentType)) errors.Add("Document type is required.");
        if (string.IsNullOrWhiteSpace(metadata.ClientId)) errors.Add("Client id is required.");
        if (!await _dataLookupClient.ClientExistsAsync(metadata.ClientId, cancellationToken)) errors.Add("Client id was not found in DataLookup.");
        return new MetadataValidationResult(errors.Count == 0, errors);
    }
}
