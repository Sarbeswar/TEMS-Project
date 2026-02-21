# Pattern Implementation Guide (CQRS + Saga + API Gateway + Circuit Breaker)

This guide explains **how to implement each required pattern step-by-step** across your microservices:
- AuthService
- ICMP.API
- DataLookupService
- IRMUploadService
- IRMDownloadService
- NotificationService
- DocumentGateway.Api

---

## 1) Before you start (baseline for all services)

For each service, keep Clean Architecture projects:
- `{Service}.Domain`
- `{Service}.Application`
- `{Service}.Infrastructure`
- `{Service}.WebAPI`

Recommended packages:
- MediatR (CQRS)
- FluentValidation
- EF Core
- MassTransit or raw Kafka client in `shared/EventBus.Kafka`
- Polly or .NET resilience (`Microsoft.Extensions.Http.Resilience`)
- Serilog + OpenTelemetry

---

## 2) CQRS – implementation at each layer

## 2.1 Domain layer
Create aggregates and domain events only. Example in `ICMP.Domain`:
- `DocumentRequest` aggregate
- `DocumentRequestCreatedDomainEvent`
- Business rules (`CanMoveToStatus`, `ValidateOwner`)

## 2.2 Application layer
Create commands and queries:
- Commands (write):
  - `CreateDocumentRequestCommand`
  - `SubmitDocumentCommand`
  - `ApproveDocumentCommand`
- Queries (read):
  - `GetRequestByIdQuery`
  - `GetRequestsByClientQuery`

Handler rules:
- Command handlers update aggregates + persist + publish integration event (through outbox).
- Query handlers read projection tables (fast read models), not full aggregate logic.

Pipeline behaviors:
- Validation behavior (FluentValidation)
- Logging behavior
- Idempotency behavior (use `RequestId`)

## 2.3 Infrastructure layer
- EF Core DbContext
- Repository implementations
- Outbox table + outbox publisher
- Query/read repositories (optimized SQL)

## 2.4 WebAPI layer
- Thin controllers/minimal API endpoints
- Endpoint receives DTO -> sends MediatR command/query -> returns response

Example endpoint flow:
1. `POST /api/document-requests`
2. controller sends `CreateDocumentRequestCommand`
3. handler saves aggregate
4. handler adds `DocumentRequestCreatedIntegrationEvent` to outbox
5. background worker publishes event to Kafka

---

## 3) Saga – implementation (orchestration in ICMP.API)

Use **orchestration saga** in `services/ICMP.API`.

## 3.1 Saga state model
Create table `DocumentRequestSagaState` with:
- `SagaId`
- `CorrelationId`
- `RequestId`
- `CurrentStep`
- `Status` (`InProgress`, `Completed`, `Failed`, `Compensating`)
- `LastError`
- `UpdatedAt`

## 3.2 Saga steps (example)
1. ICMP API creates request -> emits `DocumentRequestCreated`
2. IRMUpload handles event -> reserves upload slot -> emits success/failure
3. DataLookup validates type/rules -> emits success/failure
4. IRMDownload prepares secure access metadata (optional pre-step)
5. Notification sends initial message
6. Saga marks completed

## 3.3 Compensation flow
If any step fails:
- Emit `CancelDocumentRequest`
- Emit `ReleaseUploadReservation`
- Emit `NotifyFailure`
- Mark saga as `Failed`

## 3.4 Idempotency in saga consumers
Each consumer should store processed message IDs to prevent duplicate processing.

---

## 4) API Gateway – implementation in `gateway/DocumentGateway.Api`

Use YARP in gateway.

Gateway responsibilities:
- Single endpoint for Angular
- Route to services:
  - `/api/auth/*` -> AuthService
  - `/api/icmp/*` -> ICMP.WebAPI
  - `/api/lookup/*` -> DataLookup.WebAPI
  - `/api/upload/*` -> IRMUpload.WebAPI
  - `/api/download/*` -> IRMDownload.WebAPI
  - `/api/notify/*` -> Notification.WebAPI
- JWT validation
- Correlation ID propagation
- Rate limiting

Do **not** place business logic in gateway.

---

## 5) Circuit Breaker – where and how

Apply resilience in two places:

1. **Gateway outbound calls** to services
2. **Service-to-service HTTP clients** (if any direct sync calls exist)

Policy set:
- Timeout (2-5s)
- Retry (small, exponential backoff)
- Circuit breaker
- Optional fallback for read endpoints

Recommended threshold example:
- Break after 5 failures in 30 seconds
- Keep open for 30 seconds
- Half-open probe with limited requests

Important:
- For command endpoints, avoid unsafe retries unless idempotency key is present.

---

## 6) Service-by-service implementation map

## 6.1 AuthService
- CQRS:
  - `LoginCommand`, `RefreshTokenCommand`, `GetUserProfileQuery`
- Saga: usually not orchestrator, just token/auth events
- API Gateway: route `/api/auth/*`
- Circuit breaker: on external identity provider calls (if used)

## 6.2 ICMP.API (core orchestrator)
- CQRS:
  - `CreateDocumentRequestCommand`, `UpdateRequestStatusCommand`, `GetRequestQuery`
- Saga:
  - Owns `DocumentRequestSagaState`
  - Sends commands/events to other services
  - Handles success/failure and compensations
- API Gateway: route `/api/icmp/*`
- Circuit breaker: protect sync calls (if it calls lookup directly)

## 6.3 DataLookupService
- CQRS:
  - `CreateLookupItemCommand`, `GetLookupByTypeQuery`
- Saga participant:
  - Validates lookup constraints for request
- API Gateway: route `/api/lookup/*`
- Circuit breaker: mainly for any external master-data dependency

## 6.4 IRMUploadService
- CQRS:
  - `InitiateUploadCommand`, `CompleteUploadCommand`, `GetUploadStatusQuery`
- Saga participant:
  - Reserve upload slot and emit result events
- API Gateway: route `/api/upload/*`
- Circuit breaker: scanner/storage APIs

## 6.5 IRMDownloadService
- CQRS:
  - `GenerateDownloadTokenCommand`, `GetDocumentStreamQuery`
- Saga participant:
  - Optional pre-download setup
- API Gateway: route `/api/download/*`
- Circuit breaker: object storage/document store calls

## 6.6 NotificationService
- CQRS:
  - `SendNotificationCommand`, `GetNotificationStatusQuery`
- Saga participant:
  - Sends success/failure notifications from saga events
- API Gateway: route `/api/notify/*`
- Circuit breaker: SMTP/SMS providers

---

## 7) Suggested implementation sequence (practical)

1. Build AuthService + Gateway authentication first.
2. Build ICMP.API CQRS command/query baseline.
3. Add Kafka/EventBus and Outbox in ICMP + IRMUpload.
4. Implement saga end-to-end for create-request + upload + notify.
5. Add DataLookup and compensation flows.
6. Add IRMDownload secure token flow.
7. Add resilience policies and observability in all services.

---

## 8) Definition of done checklist

A service is considered done when:
- CQRS commands/queries exist with validation.
- Outbox is enabled for integration events.
- Consumers are idempotent.
- Health checks + logs + traces are enabled.
- API is routed through gateway.
- Resilience policies are configured for outbound dependencies.

Saga is done when:
- Happy path works end-to-end.
- At least one failure path triggers compensation.
- Duplicate event handling does not corrupt state.

## 9) Kafka message broker implementation (50 topics + partitioning)

Because your system uses Kafka heavily, implement the event bus with:

- 50 topics (commands/events/retry-dlq/audit)
- Partition key = `CorrelationId` (fallback `RequestId`)
- Outbox in producer services + Inbox/idempotency in consumers
- Retry topics + DLQ topics per major bounded context

Use this detailed catalog and topic-by-topic plan:
- `KAFKA_50_TOPICS_GUIDE.md`
- `KAFKA_CODE_STRUCTURE_GUIDE.md` (exact `KafkaEventProducer.cs`, `KafkaEventConsumer.cs`, DTO/event model placement)

