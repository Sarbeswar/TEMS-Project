namespace AIAgent.Domain.ValueObjects;

/// <summary>
/// Value object that protects the domain from invalid Azure Blob document locations.
/// </summary>
public sealed record BlobDocumentLocation(string ContainerName, string BlobName)
{
    /// <summary>Creates a validated blob location from Kafka event data.</summary>
    public static BlobDocumentLocation Create(string containerName, string blobName)
    {
        if (string.IsNullOrWhiteSpace(containerName)) throw new ArgumentException("Container name is required.", nameof(containerName));
        if (string.IsNullOrWhiteSpace(blobName)) throw new ArgumentException("Blob name is required.", nameof(blobName));
        return new BlobDocumentLocation(containerName.Trim(), blobName.Trim());
    }
}
