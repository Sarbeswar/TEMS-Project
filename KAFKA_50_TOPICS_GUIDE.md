# Kafka Message Broker Design (50 Topics + Partition Strategy)

This guide adds your required Kafka setup where payload messages are exchanged across **50 topics** with partitioning.

## 1) Cluster baseline

- Broker mode: KRaft or ZooKeeper-based (either is fine for dev; prefer KRaft for new setup).
- Replication factor:
  - Dev: `1`
  - SIT/UAT/Prod: `3`
- Partitions per topic (starting point):
  - Commands: `6`
  - Events: `12`
  - Notifications: `6`
  - Audit/DLQ/Retry: `3`
- Message format: JSON or Avro (Avro + schema registry recommended in production).
- Delivery semantics: `at-least-once` with idempotent consumers.

## 2) Topic naming standard

Use this format:

`dms.{service}.{entity}.{action}.{kind}.v1`

Examples:
- `dms.icmp.documentrequest.create.command.v1`
- `dms.irmupload.upload.completed.event.v1`
- `dms.notification.delivery.failed.event.v1`

## 3) Partition-key standard (very important)

To keep ordering per business request, always publish using one stable key:

- Primary key: `CorrelationId` (Saga flow)
- Fallback key: `RequestId`
- For notifications: `ClientId` or `UserId`

Result: all events of one request go to the same partition, preserving order for that request.

## 4) Required 50-topic catalog

## 4.1 Command topics (10)
1. `dms.icmp.documentrequest.create.command.v1`
2. `dms.icmp.documentrequest.cancel.command.v1`
3. `dms.icmp.documentrequest.approve.command.v1`
4. `dms.auth.token.issue.command.v1`
5. `dms.auth.token.refresh.command.v1`
6. `dms.lookup.validation.start.command.v1`
7. `dms.irmupload.upload.initiate.command.v1`
8. `dms.irmupload.upload.rollback.command.v1`
9. `dms.irmdownload.token.generate.command.v1`
10. `dms.notification.send.command.v1`

## 4.2 Domain/event topics (25)
11. `dms.icmp.documentrequest.created.event.v1`
12. `dms.icmp.documentrequest.approved.event.v1`
13. `dms.icmp.documentrequest.cancelled.event.v1`
14. `dms.icmp.saga.started.event.v1`
15. `dms.icmp.saga.completed.event.v1`
16. `dms.icmp.saga.failed.event.v1`
17. `dms.auth.token.issued.event.v1`
18. `dms.auth.token.refreshed.event.v1`
19. `dms.lookup.validation.passed.event.v1`
20. `dms.lookup.validation.failed.event.v1`
21. `dms.lookup.masterdata.changed.event.v1`
22. `dms.irmupload.upload.initiated.event.v1`
23. `dms.irmupload.upload.completed.event.v1`
24. `dms.irmupload.upload.failed.event.v1`
25. `dms.irmupload.file.scannedclean.event.v1`
26. `dms.irmupload.file.scannedinfected.event.v1`
27. `dms.irmdownload.token.generated.event.v1`
28. `dms.irmdownload.download.started.event.v1`
29. `dms.irmdownload.download.completed.event.v1`
30. `dms.irmdownload.download.failed.event.v1`
31. `dms.notification.delivery.requested.event.v1`
32. `dms.notification.delivery.sent.event.v1`
33. `dms.notification.delivery.failed.event.v1`
34. `dms.gateway.request.received.event.v1`
35. `dms.gateway.request.rejected.event.v1`

## 4.3 Retry + dead-letter topics (10)
36. `dms.retry.icmp.documentrequest.v1`
37. `dms.retry.lookup.validation.v1`
38. `dms.retry.irmupload.upload.v1`
39. `dms.retry.irmdownload.download.v1`
40. `dms.retry.notification.delivery.v1`
41. `dms.dlq.icmp.documentrequest.v1`
42. `dms.dlq.lookup.validation.v1`
43. `dms.dlq.irmupload.upload.v1`
44. `dms.dlq.irmdownload.download.v1`
45. `dms.dlq.notification.delivery.v1`

## 4.4 Audit + observability topics (5)
46. `dms.audit.security.event.v1`
47. `dms.audit.useractivity.event.v1`
48. `dms.audit.dataccess.event.v1`
49. `dms.obs.service.health.event.v1`
50. `dms.obs.pipeline.metrics.event.v1`

## 5) Producer/consumer ownership

- `ICMP.API` publishes request lifecycle + saga topics; consumes lookup/upload/download/notification results.
- `AuthService` publishes token events; consumes token commands.
- `DataLookupService` consumes validation command; publishes validation result.
- `IRMUploadService` consumes upload commands; publishes upload/scan events.
- `IRMDownloadService` consumes token/download commands; publishes download events.
- `NotificationService` consumes send commands; publishes delivery status.
- `DocumentGateway.Api` may publish request-level audit/observability events only (no business decisions).

## 6) Partition sizing quick guide

Start small and scale by throughput:

- Commands: 6 partitions (order + moderate throughput)
- Events: 12 partitions (high fan-out)
- Retry/DLQ/Audit: 3 partitions

When to increase partitions:
- Consumer lag is growing for sustained periods.
- Processing time per message is high and you need parallelism.

Note: increasing partitions changes key-to-partition mapping for new messages.

## 7) Message envelope (common contract)

Every message should include:
- `MessageId` (GUID)
- `CorrelationId` (Saga correlation)
- `CausationId`
- `EventType`
- `OccurredAtUtc`
- `SourceService`
- `TenantId` (if multi-tenant)
- `Payload`

## 8) Reliability controls

- Use Outbox pattern in each write service (`ICMP`, `IRMUpload`, `IRMDownload`, `Notification`).
- Use Inbox/idempotency table in each consumer (`MessageId` unique).
- Configure retry with backoff and send poisoned messages to DLQ.
- Add schema version in topic name suffix (`v1`, later `v2`).

## 9) Where to implement in this solution

- Topic constants and producer abstractions: `shared/EventBus.Kafka`
- Producer implementation + outbox dispatcher: each `*.Infrastructure`
- Consumer handlers: each `*.WebAPI` background consumer or hosted service
- Saga correlation: `services/ICMP.API` (`ICMP.Application` + `ICMP.Infrastructure`)


## 10) Implementation class/file blueprint

For exact file-level guidance on creating producer/consumer classes and DTO/event models, see:
- `KAFKA_CODE_STRUCTURE_GUIDE.md`


