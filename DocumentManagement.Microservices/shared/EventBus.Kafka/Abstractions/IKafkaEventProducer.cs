namespace EventBus.Kafka.Abstractions;

public interface IKafkaEventProducer
{
    Task PublishAsync<TEvent>(string topic, TEvent payload, string partitionKey, CancellationToken cancellationToken = default);
}
