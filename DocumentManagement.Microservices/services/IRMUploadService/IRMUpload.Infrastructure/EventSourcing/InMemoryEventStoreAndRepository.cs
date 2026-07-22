using EventBus.Kafka.Abstractions;
using IRMUpload.Application.Abstractions;
using IRMUpload.Domain.Entities;
using IRMUpload.Domain.Events;
using SharedKernel.IntegrationEvents;

namespace IRMUpload.Infrastructure.EventSourcing;

public interface IEventStore
{
    Task AppendAsync(Guid streamId, IReadOnlyCollection<IDomainEvent> events, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<IDomainEvent>> ReadAsync(Guid streamId, CancellationToken cancellationToken);
}

public sealed class InMemoryEventStore : IEventStore
{
    private readonly Dictionary<Guid, List<IDomainEvent>> _streams = new();

    public Task AppendAsync(Guid streamId, IReadOnlyCollection<IDomainEvent> events, CancellationToken cancellationToken)
    {
        if (!_streams.TryGetValue(streamId, out var stream))
        {
            stream = new List<IDomainEvent>();
            _streams[streamId] = stream;
        }

        stream.AddRange(events);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<IDomainEvent>> ReadAsync(Guid streamId, CancellationToken cancellationToken)
    {
        _streams.TryGetValue(streamId, out var stream);
        return Task.FromResult<IReadOnlyCollection<IDomainEvent>>(stream ?? new List<IDomainEvent>());
    }
}

public sealed class IrmFileRepository : IIrmFileRepository
{
    private const string IrmCreatedTopic = "irm.file.created.v1";
    private const string IrmUpdatedTopic = "irm.file.updated.v1";
    private const string IrmDeletedTopic = "irm.file.deleted.v1";

    private readonly IEventStore _eventStore;
    private readonly IKafkaEventProducer _producer;

    public IrmFileRepository(IEventStore eventStore, IKafkaEventProducer producer)
    {
        _eventStore = eventStore;
        _producer = producer;
    }

    public async Task<IrmFileAggregate?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken)
    {
        var history = await _eventStore.ReadAsync(fileId, cancellationToken);
        if (history.Count == 0)
        {
            return null;
        }

        var aggregate = new IrmFileAggregate();
        aggregate.LoadFromHistory(history);
        return aggregate;
    }

    public async Task SaveAsync(IrmFileAggregate aggregate, string correlationId, CancellationToken cancellationToken)
    {
        var pendingEvents = aggregate.PendingEvents;
        if (pendingEvents.Count == 0)
        {
            return;
        }

        await _eventStore.AppendAsync(aggregate.Id, pendingEvents, cancellationToken);

        foreach (var @event in pendingEvents)
        {
            switch (@event)
            {
                case IrmFileCreated created:
                    await _producer.PublishAsync(
                        IrmCreatedTopic,
                        new IrmFileCreatedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, correlationId, created.FileId, created.FileName, created.StoragePath, created.RequestedBy),
                        created.FileId.ToString(),
                        cancellationToken);
                    break;

                case IrmFileUpdated updated:
                    await _producer.PublishAsync(
                        IrmUpdatedTopic,
                        new IrmFileUpdatedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, correlationId, updated.FileId, updated.FileName, updated.RequestedBy),
                        updated.FileId.ToString(),
                        cancellationToken);
                    break;

                case IrmFileDeleted deleted:
                    await _producer.PublishAsync(
                        IrmDeletedTopic,
                        new IrmFileDeletedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, correlationId, deleted.FileId, deleted.RequestedBy),
                        deleted.FileId.ToString(),
                        cancellationToken);
                    break;
            }
        }

        aggregate.ClearPendingEvents();
    }
}
