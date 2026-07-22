using System.Text.Json;
using AIAgent.Application.Abstractions;
using AIAgent.Application.DTOs;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace AIAgent.Infrastructure.Kafka;

/// <summary>Kafka producer responsible for publishing AI integration events to downstream services.</summary>
public sealed class AiKafkaEventPublisher : IAiEventPublisher
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<AiKafkaEventPublisher> _logger;

    public AiKafkaEventPublisher(IProducer<string, string> producer, ILogger<AiKafkaEventPublisher> logger)
    {
        _producer = producer;
        _logger = logger;
    }

    /// <summary>Publishes successful metadata extraction for Replication and Notification consumers.</summary>
    public async Task PublishMetadataExtractedAsync(MetadataExtractedEvent integrationEvent, CancellationToken cancellationToken) =>
        await PublishAsync("ai.metadata.extracted.v1", integrationEvent.DocumentId.ToString(), integrationEvent, cancellationToken);

    /// <summary>Publishes manual-review request for Notification and work-queue consumers.</summary>
    public async Task PublishManualReviewRequiredAsync(ManualReviewRequiredEvent integrationEvent, CancellationToken cancellationToken) =>
        await PublishAsync("ai.manual-review.required.v1", integrationEvent.DocumentId.ToString(), integrationEvent, cancellationToken);

    private async Task PublishAsync<T>(string topic, string key, T payload, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload);
        await _producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = json }, cancellationToken);
        _logger.LogInformation("Published Kafka event {EventType} to {Topic}", typeof(T).Name, topic);
    }
}
