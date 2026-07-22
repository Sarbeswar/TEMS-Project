using AIAgent.Application.Abstractions;
using AIAgent.Domain.ValueObjects;
using Azure.Storage.Blobs;

namespace AIAgent.Infrastructure.BlobStorage;

/// <summary>Azure Blob Storage adapter for downloading IRM uploaded documents.</summary>
public sealed class AzureBlobStorageClient : IBlobStorageClient
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageClient(BlobServiceClient blobServiceClient) => _blobServiceClient = blobServiceClient;

    /// <summary>Downloads the document stream so OCR can run without storing files on local disk.</summary>
    public async Task<Stream> DownloadAsync(BlobDocumentLocation location, CancellationToken cancellationToken)
    {
        var blob = _blobServiceClient.GetBlobContainerClient(location.ContainerName).GetBlobClient(location.BlobName);
        var response = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value.Content;
    }
}
