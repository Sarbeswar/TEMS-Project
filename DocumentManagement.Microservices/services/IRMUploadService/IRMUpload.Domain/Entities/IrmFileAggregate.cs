using IRMUpload.Domain.Events;

namespace IRMUpload.Domain.Entities;

public class IrmFileAggregate
{
    private readonly List<IDomainEvent> _pendingEvents = new();

    public Guid Id { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string StoragePath { get; private set; } = string.Empty;
    public bool IsDeleted { get; private set; }

    public IReadOnlyCollection<IDomainEvent> PendingEvents => _pendingEvents.AsReadOnly();

    public static IrmFileAggregate Create(Guid id, string fileName, string storagePath, string requestedBy)
    {
        var aggregate = new IrmFileAggregate();
        aggregate.ApplyChange(new IrmFileCreated(id, fileName, storagePath, requestedBy, DateTime.UtcNow));
        return aggregate;
    }

    public void Rename(string fileName, string requestedBy)
        => ApplyChange(new IrmFileUpdated(Id, fileName, requestedBy, DateTime.UtcNow));

    public void Delete(string requestedBy)
        => ApplyChange(new IrmFileDeleted(Id, requestedBy, DateTime.UtcNow));

    public void LoadFromHistory(IEnumerable<IDomainEvent> history)
    {
        foreach (var @event in history)
        {
            Mutate(@event);
        }
    }

    public void ClearPendingEvents() => _pendingEvents.Clear();

    private void ApplyChange(IDomainEvent @event)
    {
        Mutate(@event);
        _pendingEvents.Add(@event);
    }

    private void Mutate(IDomainEvent @event)
    {
        switch (@event)
        {
            case IrmFileCreated created:
                Id = created.FileId;
                FileName = created.FileName;
                StoragePath = created.StoragePath;
                IsDeleted = false;
                break;
            case IrmFileUpdated updated:
                FileName = updated.FileName;
                break;
            case IrmFileDeleted:
                IsDeleted = true;
                break;
        }
    }
}
