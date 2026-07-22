# IRM File CRUD Code Practice (All 5 Patterns)

This starter gives you practical code placement for **IRM file CRUD** with your architecture.

## 1) CQRS (`IRMUpload.Application`)
- Commands and handlers:
  - `CreateIrmFileCommand`, `UpdateIrmFileCommand`, `DeleteIrmFileCommand`
  - `IrmFileCommandHandlers`
- Query and handler:
  - `GetIrmFileByIdQuery`
  - `GetIrmFileByIdQueryHandler`

## 2) Saga (`ICMP.API` orchestrator)
- Orchestration class:
  - `ICMP.Application/Sagas/IrmFileCrudSagaOrchestrator.cs`
- Flow:
  1. Create file in IRM Upload service.
  2. Notify downstream service.
  3. If notification fails, compensate by deleting created file.

## 3) API Gateway (`gateway/DocumentGateway.Api`)
- Example YARP route:
  - `Config/yarp.irmcrud.sample.json`
- Route forwards `/api/irm/**` to IRM Upload WebAPI and injects correlation header.

## 4) Circuit Breaker (Gateway + service HTTP clients)
- `yarp.irmcrud.sample.json` includes resiliency settings example:
  - timeout, retry, and circuit breaker thresholds.
- Apply equivalent `.AddResilienceHandler(...)` or Polly settings to outbound clients in services.

## 5) Event Sourcing (`IRMUpload.Domain` + `IRMUpload.Infrastructure`)
- Domain events + aggregate:
  - `IRMUpload.Domain/Events/IrmFileDomainEvents.cs`
  - `IRMUpload.Domain/Entities/IrmFileAggregate.cs`
- Event store + repository:
  - `IRMUpload.Infrastructure/EventSourcing/InMemoryEventStoreAndRepository.cs`
- Repository appends domain events and publishes integration events to Kafka.

## Integration contracts
- Kafka producer abstraction:
  - `shared/EventBus.Kafka/Abstractions/IKafkaEventProducer.cs`
- Shared integration events:
  - `shared/SharedKernel/IntegrationEvents/IrmFileIntegrationEvents.cs`

## API layer
- CRUD endpoints:
  - `IRMUpload.WebAPI/Controllers/IrmFilesController.cs`

## Next step recommendation
- Replace in-memory event store with database-backed store (SQL/EventStoreDB).
- Add idempotency key handling + outbox table.
- Add authentication/authorization through AuthService token validation.


## Pattern coverage and benefits (what is implemented)

### CQRS
**Implemented in:**
- `IRMUpload.Application/Commands/IrmFileCommands.cs`
- `IRMUpload.Application/Queries/GetIrmFileByIdQuery.cs`

**Benefit:**
- Separates write logic (create/update/delete) from read logic (get by id), making handlers simpler and easier to scale independently.

### Saga
**Implemented in:**
- `ICMP.Application/Sagas/IrmFileCrudSagaOrchestrator.cs`

**Benefit:**
- Coordinates distributed steps across microservices and applies compensation (rollback-style action) when a downstream step fails.

### API Gateway
**Implemented in:**
- `gateway/DocumentGateway.Api/Config/yarp.irmcrud.sample.json`

**Benefit:**
- Single entry point for Angular/client apps, centralized routing, and easier enforcement of auth/correlation/observability policies.

### Circuit Breaker
**Implemented in:**
- `gateway/DocumentGateway.Api/Config/yarp.irmcrud.sample.json` (`Resilience` sample)

**Benefit:**
- Prevents cascading failures by quickly short-circuiting unstable downstream calls and allowing recovery windows.

### Event Sourcing
**Implemented in:**
- `IRMUpload.Domain/Entities/IrmFileAggregate.cs`
- `IRMUpload.Domain/Events/IrmFileDomainEvents.cs`
- `IRMUpload.Infrastructure/EventSourcing/InMemoryEventStoreAndRepository.cs`

**Benefit:**
- Keeps full history of file state transitions, supports replay/projections, and improves auditability for IRM operations.

### Kafka event publishing (integration pattern)
**Implemented in:**
- `shared/EventBus.Kafka/Abstractions/IKafkaEventProducer.cs`
- `shared/SharedKernel/IntegrationEvents/IrmFileIntegrationEvents.cs`
- `IRMUpload.Infrastructure/EventSourcing/InMemoryEventStoreAndRepository.cs`

**Benefit:**
- Decouples services via asynchronous events, supports independent consumers, and improves horizontal scalability.
