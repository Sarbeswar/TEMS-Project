using System.Net;

namespace ICMP.Application.Sagas;

public interface IIrmUploadClient
{
    Task<HttpStatusCode> CreateFileAsync(Guid fileId, string fileName, string storagePath, string requestedBy, string correlationId, CancellationToken cancellationToken);
    Task<HttpStatusCode> DeleteFileAsync(Guid fileId, string requestedBy, string correlationId, CancellationToken cancellationToken);
}

public interface INotificationClient
{
    Task NotifyAsync(string message, string correlationId, CancellationToken cancellationToken);
}

public class IrmFileCrudSagaOrchestrator
{
    private readonly IIrmUploadClient _irmUploadClient;
    private readonly INotificationClient _notificationClient;

    public IrmFileCrudSagaOrchestrator(IIrmUploadClient irmUploadClient, INotificationClient notificationClient)
    {
        _irmUploadClient = irmUploadClient;
        _notificationClient = notificationClient;
    }

    public async Task<bool> RunCreateSagaAsync(Guid fileId, string fileName, string storagePath, string requestedBy, string correlationId, CancellationToken cancellationToken)
    {
        var createStatus = await _irmUploadClient.CreateFileAsync(fileId, fileName, storagePath, requestedBy, correlationId, cancellationToken);
        if (createStatus is not HttpStatusCode.Created and not HttpStatusCode.Accepted)
        {
            return false;
        }

        try
        {
            await _notificationClient.NotifyAsync($"IRM file {fileName} created", correlationId, cancellationToken);
            return true;
        }
        catch
        {
            // Compensation step in Saga if next step fails.
            await _irmUploadClient.DeleteFileAsync(fileId, requestedBy, correlationId, cancellationToken);
            return false;
        }
    }
}
