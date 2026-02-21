# Document Request Record Management – Target Architecture (.NET + Angular)

## 1) Goal
Recreate a **Document Request Record Management** platform with:
- Angular (latest) client app
- .NET microservices:
  - DataLookup
  - IRMUpload
  - IRMDownload
  - ICMPNotification
  - ICMPAPI
  - AuthServiceToken
- Required patterns:
  - CQRS
  - Saga
  - API Gateway
  - Circuit Breaker
- Project layout aligned to your attached **Clean Architecture** style.

---

## 2) Recommended high-level architecture

```text
[Angular App]
    |
    v
[API Gateway (YARP/Ocelot)]  <-- JWT validation (or forward to Auth service)
    |
    +--> [ICMPAPI] -------------------------+
    |                                       |
    +--> [DataLookup]                       |
    +--> [IRMUpload]                        |  (async events via broker)
    +--> [IRMDownload]                      +--> [Message Broker: RabbitMQ/Azure Service Bus]
    +--> [ICMPNotification]                 |
    +--> [AuthServiceToken] ----------------+

Per service DB (database-per-service), plus object/file storage for documents.
```

### Service responsibilities
- **AuthServiceToken**: login, JWT issuance, refresh token, token introspection.
- **ICMPAPI**: BFF/domain-facing API for document request workflow; starts Saga.
- **DataLookup**: master/reference data (document types, status, lookups).
- **IRMUpload**: upload API, malware scan trigger, metadata registration.
- **IRMDownload**: secure download with access checks and audit.
- **ICMPNotification**: email/SMS/in-app notification events.

---

## 3) Clean Architecture project layout (match attached structure)
For each microservice, keep this solution structure:

```text
src/
  {service}.Domain
  {service}.Application
  {service}.Infrastructure
  {service}.WebApi
tests/
  {service}.UnitTests
  {service}.IntegrationTests
```

### Layer usage
1. **Domain**
   - Aggregates, entities, value objects, domain events, business invariants.
2. **Application**
   - CQRS commands/queries, handlers, DTOs, validators, interfaces.
3. **Infrastructure**
   - EF Core, repositories, broker publisher/subscriber, external integrations.
4. **WebApi (Presentation)**
   - Controllers/Minimal APIs, auth filters, API contracts.

> Keep dependency direction inward: `WebApi -> Application -> Domain`, and `Infrastructure -> Application/Domain` via interfaces.

---

## 4) CQRS implementation approach
Use MediatR in each service:

- **Commands** (write side)
  - `CreateDocumentRequestCommand`
  - `UploadDocumentCommand`
  - `ApproveDocumentCommand`
- **Queries** (read side)
  - `GetDocumentRequestByIdQuery`
  - `GetDocumentListQuery`

### Storage model
- Start simple with one DB per service + dedicated read projections.
- For high read scale, maintain materialized read tables (or separate read DB schema).

### Cross-cutting pipeline behaviors
- Validation behavior (FluentValidation)
- Logging behavior
- Idempotency behavior (for command retries)

---

## 5) Saga pattern (document request lifecycle)
Use **orchestration Saga** centered in `ICMPAPI` (or separate workflow/orchestrator service later).

### Sample saga flow
1. `CreateDocumentRequest` in ICMPAPI
2. Publish `DocumentRequestCreated`
3. IRMUpload reserves upload slot, then publishes `UploadAccepted` or `UploadRejected`
4. DataLookup validates business lookup rules, publishes `LookupValidated/Failed`
5. ICMPAPI updates workflow state
6. ICMPNotification sends user notification
7. On any failure, publish compensating events:
   - `CancelDocumentRequest`
   - `RevokeUploadReservation`
   - `NotifyFailure`

### Saga persistence
Store saga state table:
- `SagaId`, `CorrelationId`, `CurrentStep`, `Status`, `LastUpdated`, `Error`

### Eventing guidelines
- Use Outbox pattern in each service to avoid lost events.
- Include correlation ID + causation ID in all events.
- Make consumers idempotent (message dedup table).

---

## 6) API Gateway pattern
Use **YARP** (recommended in .NET ecosystem) or Ocelot.

Gateway responsibilities:
- Single entry point for Angular.
- Routing to microservices.
- JWT auth validation.
- Rate limiting.
- Request/response transformation.
- Correlation ID propagation.

Keep business logic in downstream services, not gateway.

---

## 7) Circuit Breaker & resilience
Use Polly (or .NET 8 resilience package) in all service-to-service HTTP calls.

Recommended policies:
- Timeout (2–5 sec per dependency)
- Retry with exponential backoff (short bursts)
- Circuit Breaker (open after threshold failures)
- Bulkhead isolation (protect thread pool/resources)
- Fallback response where possible (especially DataLookup read paths)

Also apply retries cautiously for commands to avoid duplicates; pair with idempotency keys.

---

## 8) Security & compliance
- OAuth2/OIDC style token model in AuthServiceToken.
- JWT access token with scoped claims (`document.read`, `document.write`, `admin`).
- Encrypt data at rest + in transit.
- Audit trail for upload/download actions.
- Signed URL or short-lived token for download endpoints.

---

## 9) Observability & operations
- Centralized structured logging (Serilog + ELK/OpenSearch/AppInsights).
- Distributed tracing with OpenTelemetry.
- Metrics dashboards (request latency, error %, broker lag, saga duration).
- Health checks per service and dependency.

---

## 10) Suggested repository structure

```text
/Artifacts
  build.proj
  build.props
  cicd.yml
  manifest.yml
  README.md
/src
  gateway.Api
  authservicetoken.Domain
  authservicetoken.Application
  authservicetoken.Infrastructure
  authservicetoken.WebApi
  icmpapi.Domain
  icmpapi.Application
  icmpapi.Infrastructure
  icmpapi.WebApi
  datalookup.Domain
  datalookup.Application
  datalookup.Infrastructure
  datalookup.WebApi
  irmupload.Domain
  irmupload.Application
  irmupload.Infrastructure
  irmupload.WebApi
  irmdownload.Domain
  irmdownload.Application
  irmdownload.Infrastructure
  irmdownload.WebApi
  icmpnotification.Domain
  icmpnotification.Application
  icmpnotification.Infrastructure
  icmpnotification.WebApi
/tests
  ... UnitTests / IntegrationTests per service
```

---

## 11) Phased implementation roadmap
1. **Foundation**: gateway, auth, ICMPAPI skeleton, shared logging/tracing.
2. **Core workflow**: create request + upload + download with saga + outbox.
3. **Reference data + notifications**: DataLookup + ICMPNotification integration.
4. **Hardening**: circuit breaker, retries, idempotency, observability, security hardening.
5. **Scale**: split read models, tune broker partitions, cache lookup data.

---

## 12) Tech stack suggestion (pragmatic)
- .NET 8, ASP.NET Core Web API
- MediatR + FluentValidation
- EF Core + SQL Server/PostgreSQL
- RabbitMQ or Azure Service Bus
- YARP API Gateway
- Polly / Microsoft.Extensions.Http.Resilience
- Serilog + OpenTelemetry
- Angular latest + NgRx (optional for complex state)

This architecture keeps your old style (Clean Architecture + DDD orientation) while modernizing reliability and scale for document workflow microservices.
