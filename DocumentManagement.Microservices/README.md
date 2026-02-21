# DocumentManagement.Microservices

## Suggested solution name
**DocumentManagement.Microservices.sln**

This repository now follows your requested Visual Studio / .NET clean-architecture microservice layout.

## Target folder structure

```text
DocumentManagement.Microservices
│
├── gateway
│   └── DocumentGateway.Api
│
├── services
│   ├── AuthService
│   │   ├── AuthService.Domain
│   │   ├── AuthService.Application
│   │   ├── AuthService.Infrastructure
│   │   └── AuthService.WebAPI
│   │
│   ├── ICMP.API
│   │   ├── ICMP.Domain
│   │   ├── ICMP.Application
│   │   ├── ICMP.Infrastructure
│   │   └── ICMP.WebAPI
│   │
│   ├── DataLookupService
│   │   ├── DataLookup.Domain
│   │   ├── DataLookup.Application
│   │   ├── DataLookup.Infrastructure
│   │   └── DataLookup.WebAPI
│   │
│   ├── IRMUploadService
│   │   ├── IRMUpload.Domain
│   │   ├── IRMUpload.Application
│   │   ├── IRMUpload.Infrastructure
│   │   └── IRMUpload.WebAPI
│   │
│   ├── IRMDownloadService
│   │   ├── IRMDownload.Domain
│   │   ├── IRMDownload.Application
│   │   ├── IRMDownload.Infrastructure
│   │   └── IRMDownload.WebAPI
│   │
│   └── NotificationService
│       ├── Notification.Domain
│       ├── Notification.Application
│       ├── Notification.Infrastructure
│       └── Notification.WebAPI
│
├── shared
│   ├── SharedKernel
│   └── EventBus.Kafka
│
├── frontend
│   └── Angular.Client
│
├── tests
│
├── docker
│
└── cicd
    ├── Jenkinsfile
    └── cicd.yml
```

## Visual Studio creation order (recommended)
1. Create blank solution: `DocumentManagement.Microservices.sln`.
2. Create `gateway/DocumentGateway.Api` as ASP.NET Core Web API.
3. For each service, create 4 projects:
   - `*.Domain` (Class Library)
   - `*.Application` (Class Library)
   - `*.Infrastructure` (Class Library)
   - `*.WebAPI` (ASP.NET Core Web API)
4. Create `shared/SharedKernel` and `shared/EventBus.Kafka` as class libraries.
5. Add all projects into solution folders matching this same tree.
6. Add project references with clean architecture direction:
   - `Application -> Domain`
   - `Infrastructure -> Application + Domain`
   - `WebAPI -> Application (+ Infrastructure via DI registration)`

## Pattern mapping
- **CQRS**: inside each `*.Application` (commands/queries/handlers).
- **Saga**: orchestrator in `services/ICMP.API`.
- **API Gateway**: `gateway/DocumentGateway.Api`.
- **Circuit Breaker**: in gateway + each service external HTTP client (Polly/.NET resilience).
- **Kafka Event Bus**: shared contract + producer/consumer abstractions in `shared/EventBus.Kafka` with 50-topic design and partition key strategy.

## Detailed implementation guide
- See `../PATTERN_IMPLEMENTATION_GUIDE.md` for step-by-step implementation across all services and layers.
- Kafka 50-topic and partition plan: `../KAFKA_50_TOPICS_GUIDE.md`.
Kafka producer/consumer + DTO placement blueprint: `../KAFKA_CODE_STRUCTURE_GUIDE.md`.


## Detailed implementation guide
- See `../PATTERN_IMPLEMENTATION_GUIDE.md` for step-by-step implementation across all services and layers.

-- Interview prep: `../INTERVIEW_PREPARATION_GUIDE.md` (how to explain architecture in interviews).
