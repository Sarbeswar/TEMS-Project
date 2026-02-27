# Kafka Producer/Consumer Implementation Blueprint (.NET Microservices)

This file answers exactly:
- Which service should create producer/consumer code?
- Where to create `KafkaEventProducer.cs` and `KafkaEventConsumer.cs`?
- Where DTO/event model classes should live?

## 1) Recommended file placement

Create the shared Kafka library in:

- `DocumentManagement.Microservices/shared/EventBus.Kafka`

Inside that, create:

```text
shared/EventBus.Kafka
├── Abstractions
│   ├── IKafkaEventProducer.cs
│   └── IKafkaEventConsumer.cs
├── Configuration
│   └── KafkaOptions.cs
├── Producers
│   └── KafkaEventProducer.cs
├── Consumers
│   ├── KafkaEventConsumer.cs
│   └── KafkaConsumerHostedService.cs
├── Serialization
│   └── JsonMessageSerializer.cs
└── TopicRegistry
    └── KafkaTopics.cs
```

## 2) Where DTO/Event models should be created

Keep event contracts in a shared contracts location so all services use the same schema.

Recommended:

```text
shared/SharedKernel
└── IntegrationEvents
    ├── Base
    │   └── IntegrationEvent.cs
    ├── ICMP
    │   ├── DocumentRequestCreatedEvent.cs
    │   ├── DocumentRequestCancelledEvent.cs
    │   └── SagaFailedEvent.cs
    ├── IRMUpload
    │   ├── UploadInitiatedEvent.cs
    │   ├── UploadCompletedEvent.cs
    │   └── UploadFailedEvent.cs
    ├── DataLookup
    │   ├── LookupValidationPassedEvent.cs
    │   └── LookupValidationFailedEvent.cs
    └── Notification
        ├── NotificationRequestedEvent.cs
        ├── NotificationSentEvent.cs
        └── NotificationFailedEvent.cs
```

If you prefer stricter ownership, contracts can instead be grouped per service under each `*.Application/Contracts/Events`, but then versioning and cross-service references become harder.

## 3) Which service creates producer and consumer?

Short answer: **every service can have both**, depending on responsibility.

- `ICMP.API`
  - Producer: publishes saga start/complete/fail and request lifecycle events.
  - Consumer: listens to upload/lookup/download/notification results.
- `AuthService`
  - Producer: token issued/refreshed events.
  - Consumer: token command topics.
- `DataLookupService`
  - Consumer: validation start command.
  - Producer: validation passed/failed event.
- `IRMUploadService`
  - Consumer: initiate/rollback upload commands.
  - Producer: upload status + scan events.
- `IRMDownloadService`
  - Consumer: generate token/download commands.
  - Producer: token generated/download status events.
- `NotificationService`
  - Consumer: send notification command.
  - Producer: delivery sent/failed events.

## 4) Exact class responsibilities

## 4.1 `IKafkaEventProducer.cs`
- Generic publish abstraction used by application/infrastructure handlers.
- Method example:
  - `Task PublishAsync<T>(string topic, string key, T message, CancellationToken ct)`

## 4.2 `KafkaEventProducer.cs`
- Concrete implementation using Confluent.Kafka producer.
- Adds headers (`CorrelationId`, `MessageId`, `EventType`).
- Serializes payload and writes to topic.

## 4.3 `IKafkaEventConsumer.cs`
- Abstraction for consuming/dispatching messages.
- Method example:
  - `Task StartAsync(CancellationToken ct)`

## 4.4 `KafkaEventConsumer.cs`
- Subscribes to topic(s), deserializes payload, dispatches to handler.
- Performs deduplication/idempotency check (`MessageId`).
- On failure: retry policy then push to DLQ.

## 4.5 `KafkaConsumerHostedService.cs`
- ASP.NET Core `BackgroundService` wrapper to run consumers with service lifecycle.

## 5) Suggested code ownership per layer

- `*.Application`
  - Defines command/query handlers and event handling contracts.
- `*.Infrastructure`
  - Implements Kafka producer/consumer wiring, serialization, topic configuration, outbox dispatcher.
- `*.WebAPI`
  - Registers hosted consumers via DI (`AddHostedService<KafkaConsumerHostedService>`).
- `shared/EventBus.Kafka`
  - Reusable library used by all services.

## 6) Minimal interface samples

```csharp
// shared/EventBus.Kafka/Abstractions/IKafkaEventProducer.cs
public interface IKafkaEventProducer
{
    Task PublishAsync<T>(string topic, string key, T payload, CancellationToken cancellationToken = default);
}
```

```csharp
// shared/EventBus.Kafka/Abstractions/IKafkaEventConsumer.cs
public interface IKafkaEventConsumer
{
    Task StartAsync(CancellationToken cancellationToken = default);
}
```

```csharp
// shared/SharedKernel/IntegrationEvents/Base/IntegrationEvent.cs
public abstract record IntegrationEvent(
    Guid MessageId,
    string CorrelationId,
    string CausationId,
    DateTime OccurredAtUtc,
    string SourceService
);
```

## 7) DI registration approach

Each service `*.WebAPI`:
- Register Kafka options from configuration.
- Register `IKafkaEventProducer -> KafkaEventProducer`.
- Register needed consumers.
- Start consumer hosted service.

Example registration flow:
1. `builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));`
2. `builder.Services.AddSingleton<IKafkaEventProducer, KafkaEventProducer>();`
3. `builder.Services.AddSingleton<IKafkaEventConsumer, KafkaEventConsumer>();`
4. `builder.Services.AddHostedService<KafkaConsumerHostedService>();`

## 8) Naming clarity for your requested files

You asked for files like `kafkaeventprodicer.cs` and `kafkaeventconser`.
Use corrected names:
- `KafkaEventProducer.cs`
- `KafkaEventConsumer.cs`

And place them in `shared/EventBus.Kafka/Producers` and `shared/EventBus.Kafka/Consumers`.

