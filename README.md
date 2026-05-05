# Email Service
Send emails easily with the help of the service. Enable bulk email sending, tracking and scheduling. Incorporate easily with multi-tenant accessibility.
The service will be available in several languages and is intended for performance and resource-management testing.

# Folder structure
```
EmailPlatform/
├── src/
│   ├── Services/
│   │   ├── TenantManagement/
│   │   ├── Identity/
│   │   ├── EmailSending/
│   │   ├── TemplateManagement/
│   │   ├── DeliveryEvents/         # SendGrid webhook ingestion
│   │   └── Notifications/          # WebSocket/SignalR hub
│   │
│   ├── Gateway/
│   │   └── ApiGateway/             # YARP or Ocelot
│   │
│   ├── BuildingBlocks/             # shared libraries
│   │   ├── Common/                 # base types, result, guards
│   │   ├── Multitenancy/           # ITenantContext, middleware, base repo
│   │   ├── Messaging/              # RabbitMQ abstractions, outbox
│   │   ├── Authentication/         # JWT/API key validation
│   │   ├── Observability/          # logging, tracing, metrics
│   │   └── Contracts/              # integration events, DTOs shared across services
│   │
│   └── Workers/
│       ├── EmailSending.Worker/    # consumes queue, calls SendGrid
│       ├── Outbox.Dispatcher/      # publishes outbox messages
│       └── Provisioning.Worker/    # handles tenant provisioning saga
│
├── tests/
│   ├── UnitTests/
│   │   ├── TenantManagement.UnitTests/
│   │   ├── EmailSending.UnitTests/
│   │   └── ...
│   ├── IntegrationTests/
│   │   ├── TenantManagement.IntegrationTests/
│   │   └── ...
│   └── TenantIsolation.Tests/      # the cross-tenant leak test suite
│
├── deploy/
│   ├── docker/
│   │   └── docker-compose.yml      # local dev: mongo, rabbit, services
│   ├── k8s/
│   └── terraform/
│
├── docs/
│   ├── architecture/
│   ├── adr/                        # architecture decision records
│   └── api/
│
├── EmailPlatform.sln
├── Directory.Build.props           # shared MSBuild settings
├── Directory.Packages.props        # central package management
├── .editorconfig
└── README.md
```

# Email service inside folder structure
```
Services/EmailSending/
├── EmailSending.Api/                # entry point, controllers, middleware
│   ├── Controllers/
│   │   ├── EmailsController.cs
│   │   └── HealthController.cs
│   ├── Middleware/
│   │   └── TenantResolutionMiddleware.cs
│   ├── Filters/
│   ├── Program.cs
│   ├── appsettings.json
│   └── EmailSending.Api.csproj
│
├── EmailSending.Application/        # use cases, no infrastructure
│   ├── Emails/
│   │   ├── Commands/
│   │   │   ├── QueueEmail/
│   │   │   │   ├── QueueEmailCommand.cs
│   │   │   │   ├── QueueEmailHandler.cs
│   │   │   │   └── QueueEmailValidator.cs
│   │   │   └── CancelScheduledEmail/
│   │   └── Queries/
│   │       ├── GetEmailById/
│   │       └── ListEmailsForTenant/
│   ├── Abstractions/                # interfaces the domain needs
│   │   ├── IEmailRepository.cs
│   │   ├── IEmailQueuePublisher.cs
│   │   └── IOutboxRepository.cs
│   ├── Behaviors/                   # MediatR pipeline (validation, logging)
│   └── EmailSending.Application.csproj
│
├── EmailSending.Domain/             # pure domain, no dependencies
│   ├── Entities/
│   │   ├── Email.cs
│   │   └── EmailAttachment.cs
│   ├── ValueObjects/
│   │   ├── EmailAddress.cs
│   │   └── TenantId.cs
│   ├── Events/                      # domain events (intra-service)
│   │   └── EmailQueuedDomainEvent.cs
│   ├── Enums/
│   │   └── EmailStatus.cs
│   ├── Exceptions/
│   └── EmailSending.Domain.csproj
│
└── EmailSending.Infrastructure/     # MongoDB, RabbitMQ, SendGrid, etc.
    ├── Persistence/
    │   ├── MongoDbContext.cs
    │   ├── Repositories/
    │   │   └── EmailRepository.cs   # inherits TenantScopedRepository<Email>
    │   └── Configurations/
    │       └── EmailMongoConfiguration.cs
    ├── Messaging/
    │   ├── EmailQueuePublisher.cs
    │   └── EventConsumers/
    ├── ExternalServices/
    │   └── SendGridClient.cs
    └── EmailSending.Infrastructure.csproj
```