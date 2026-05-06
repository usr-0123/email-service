# Email Platform — API

This folder contains the .NET 8 microservices solution that powers the Email Platform backend. See the root [README.md](../README.md) for product context.

## Solution layout

```
api/
├── EmailPlatform.sln
├── Directory.Build.props          # shared C# settings (nullable, langversion)
├── Directory.Packages.props       # central NuGet versions
├── .editorconfig
├── docker-compose.dev.yml         # OTel collector, Grafana, Loki, Tempo, Prometheus
├── src/
│   ├── Gateway/
│   │   └── EmailPlatform.Gateway/                # YARP, tenant-facing
│   ├── Services/
│   │   ├── Identity/{Api,Application,Domain,Infrastructure}/
│   │   ├── Tenants/{Api,Application,Domain,Infrastructure}/
│   │   ├── EmailSending/{Api,Application,Domain,Infrastructure,Worker}/
│   │   ├── DeliveryEvents/{Api,Application,Domain,Infrastructure}/
│   │   ├── Billing/{Api,Application,Domain,Infrastructure}/
│   │   ├── Notifications/{Api,Application,Domain,Infrastructure}/
│   │   └── Admin/{Api,Application,Domain,Infrastructure}/
│   └── BuildingBlocks/
│       ├── Common/                 # Result<T>, guards, value-object base
│       ├── Multitenancy/           # ITenantContext, middleware, TenantScopedRepository<T>
│       ├── Messaging/              # MassTransit setup, outbox interfaces, integration-event base
│       ├── Authentication/         # JWT + API-key auth handlers
│       ├── Observability/          # Serilog + OpenTelemetry wiring, log enrichment
│       ├── Persistence.Mongo/      # MongoDbContext base, conventions
│       └── Contracts/              # integration events + DTOs shared across services
└── tests/
    ├── UnitTests/                  # one project per service
    ├── IntegrationTests/           # Testcontainers-driven, one per service
    └── TenantIsolation.Tests/      # cross-tenant leak suite
```

Each service follows Clean Architecture (Api → Application → Domain → Infrastructure) so dependencies point inward and the Domain remains pure.

## Services (day-1)

| Service | Responsibility | Owns DB |
|---|---|---|
| `Identity.Api` | JWT issuance for tenant users; API-key validation; admin pool (separate collection, separate signing key) | `identity` |
| `Tenants.Api` | Tenant lifecycle, settings, sender config, API keys. **Folds Recipients, Audiences, Templates, Suppressions for v1** (extracted later). | `tenants` |
| `EmailSending.Api` | `POST /v1/emails` (transactional), `POST /v1/campaigns` (promotional, audienceId-based). Pre-flight balance check, outbox + queue. | `emailsending` |
| `EmailSending.Worker` | Drains `email.send.requested`, renders with Scriban, ships to SendGrid, updates status. **Also runs the campaign scheduler.** | shares `emailsending` |
| `DeliveryEvents.Api` | Public SendGrid webhook endpoint with signature validation; emits integration events. | `deliveryevents` |
| `Billing.Api` | Coin ledger (account + transactions), Stripe top-up, balance pre-flight endpoint, low-balance alerts. | `billing` |
| `Notifications.Api` | Outbound webhook subscriptions + delivery worker (HMAC, retries) + SignalR hub for UI real-time. | `notifications` |
| `Admin.Api` | Tenant management, metrics aggregation. Separate user pool, separate hostname. | `admin` |
| `EmailPlatform.Gateway` | YARP, tenant-facing entry. JWT/API-key validation, per-tenant rate limit, regional routing stub. | none |

## Library picks

| Concern | Pick | Reason |
|---|---|---|
| Mediation | **MediatR v11** (last MIT) — or Mediator source-gen | v12 went commercial |
| Messaging | **MassTransit v8** | last MIT version (v9 commercial); built-in retry, outbox, consumer pipeline |
| Validation | FluentValidation | plays nicely with MediatR pipeline |
| Mongo | `MongoDB.Driver` (native) | EF Core Mongo provider is immature |
| Templating | **Scriban** | Liquid-style, MIT, fastest .NET text-templating |
| Resilience | Polly v8 (`Microsoft.Extensions.Http.Resilience`) | retries on SendGrid / Stripe |
| Logging | Serilog + `Serilog.Sinks.OpenTelemetry` | structured, OTel-native |
| Telemetry | OpenTelemetry .NET SDKs + auto-instrumentation | exports OTLP to local collector |
| JWT | `Microsoft.AspNetCore.Authentication.JwtBearer` + custom issuance | simple v1; OIDC later if needed |
| Rate limiting | .NET 7+ built-in rate limiter, keyed on tenantId | per-tenant cap; in-memory v1 |
| Stripe | `Stripe.net` SDK | official |
| SendGrid | `SendGrid` SDK in **sandbox mode** | no actual delivery in dev |
| API docs | Swashbuckle | per-service Swagger UI |
| Tests | xUnit, FluentAssertions, Testcontainers, NBomber | standard |

## Multi-tenancy

- Shared collections, every document carries `tenantId` (always first in compound indexes).
- `BuildingBlocks/Multitenancy.TenantScopedRepository<T>` silently injects a `tenantId` filter on every Find / Update / Delete. Bypassing requires a deliberate `IPlatformAdminRepository<T>`.
- `ITenantContext` is set by middleware, populated from the JWT claim or by hashed-API-key lookup.
- `tests/TenantIsolation.Tests/` asserts no documented endpoint, given Tenant A's credentials, can return Tenant B's data.

## Multi-region

Every tenant has a `homeRegion` (e.g. `"ke-1"`). The gateway reads it from the JWT and routes to the matching cell. v1 ships **one cell only (AWS Cape Town)**, but the field exists from day one. Identity is the single global service; everything else is per-region.

## RabbitMQ topology

Integration events (fanout exchanges, consumed where relevant):

- `email.send.requested` → `EmailSending.Worker`
- `email.delivery.recorded` → `Notifications` (fan-out + SignalR), `Tenants` (suppression update on bounce/complaint/unsubscribe), `Admin` (dashboard)
- `billing.coins.debited`, `billing.balance.low` → `Notifications`
- `tenant.created`, `tenant.suspended` → all interested

**Scheduling.** Mongo-polling scheduler inside `EmailSending.Worker`. A timer fires every 30s, picks campaigns where `nextOccurrenceAt <= now`, expands the audience, dedupes, intersects suppressions, runs the atomic balance check, debits coins, then enqueues one `email.send.requested` per recipient. Recurrence rules computed in code (Cronos). Chosen over RabbitMQ delayed messages for trivial cancellation and native recurrence.

**Outbox.** Transactional outbox pattern. APIs write business doc + outbox doc in one Mongo session; an in-process dispatcher flips outbox docs onto RabbitMQ. Standalone `Outbox.Dispatcher` worker only if contention shows up.

## Observability

```
Each service ──OTLP──▶ OTel Collector ──┬──▶ Loki        (logs)
                                        ├──▶ Tempo       (traces)
                                        └──▶ Prometheus  (metrics)
                                              ▲
                                              └──── Grafana
```

- Serilog enriches every log line with `TenantId`, `TraceId`, `ServiceName`.
- Auto-instrumentation: ASP.NET Core, HttpClient, MassTransit, MongoDB.Driver.
- Pre-provisioned Grafana dashboards: per-tenant send rate, queue depth, SendGrid latency, coin debits/sec, p50/p95/p99 API latency, error rate.

## MVP — definition of done

The first runnable end-to-end slice is reached when all of the following pass against `docker compose up`:

1. Seed script creates 1 tenant, 1 user, 1 API key, 1 audience with 3 recipients, 1 template (en-US), 100 seeded coins.
2. `POST /v1/emails` (transactional) with API key → SendGrid sandbox → `GET /v1/emails/{id}` shows status.
3. `POST /v1/campaigns` (promotional, audienceId, `scheduledAt: null`) → 3 emails enqueued, dedupe + suppression intersect verified, all reach SendGrid sandbox.
4. SendGrid sandbox webhook → `DeliveryEvents.Api` → email status updates to `delivered`.
5. Sending email worth 5 coins when balance = 3 returns 402 with `insufficient credits`; no debit, no partial send.
6. Tenant subscribes a webhook URL → receives HMAC-signed POST on `email.delivered`.
7. Logs / traces / metrics visible in Grafana.
8. `TenantIsolation.Tests` passes.
9. NBomber meets perf target: 100 emails/sec sustained, `POST /v1/emails` p99 < 300ms.

## Phased build order

| Phase | Output |
|---|---|
| 0 Foundation | Solution, `Directory.*.props`, BuildingBlocks skeletons, `docker-compose.dev.yml` with telemetry stack, healthcheck visible in Grafana |
| 1 Identity + Tenants | JWT issuance, API-key generation, tenant CRUD, Recipients/Audiences/Templates/Suppressions CRUD, tenant-resolution middleware, seed script |
| 2 Billing | Coin ledger, Stripe top-up (test mode), pre-flight check, low-balance alert publisher |
| 3 EmailSending hot path | `POST /v1/emails` → outbox → Rabbit → Worker → SendGrid sandbox → status (MVP items 1, 2) |
| 4 DeliveryEvents + auto-suppression | SendGrid webhook receiver + fan-out; suppression auto-update (MVP items 3, 4) |
| 5 Notifications | Outbound webhooks (subscriptions + delivery worker + HMAC + retries), SignalR hub (MVP item 6) |
| 6 Campaigns + scheduler + recurrence | `POST /v1/campaigns` with `scheduledAt` and `recurrenceRule`, Mongo-polling scheduler, audience expansion + dedupe + suppression intersect + atomic coin debit |
| 7 Per-tenant rate limit + global throttle | Rate limit at gateway, worker concurrency tuning |
| 8 Admin.Api + Gateway | YARP gateway, admin user pool, admin dashboard endpoints |
| 9 i18n | Email content i18n via `(templateKey, locale)` keyed off `recipient.locale` |
| 10 Tests + perf | Cross-tenant leak suite, NBomber benchmarks for target (a) (MVP items 7, 8, 9) |

## Running locally

> Not yet wired up — Phase 0 produces the solution scaffold and `docker-compose.dev.yml`. Until then, only the shared infra in the root `docker-compose.yml` (MongoDB + RabbitMQ) is runnable.

Once Phase 0 ships:

```bash
docker compose -f ../docker-compose.yml -f docker-compose.dev.yml up -d
dotnet build EmailPlatform.sln
dotnet run --project src/Services/Identity/Identity.Api
```

## Open decisions tracked for later phases

- Coin name (placeholder: `EmailCredits`).
- Refund policy when SendGrid rejects pre-acceptance.
- `recurrenceRule` shape: cron string vs. structured RFC 5545 subset.
- Per-tenant rate-limit default (e.g. 60/min).
- Admin user creation flow: CLI vs. bootstrap-from-env.
- 75% warning baseline: configurable absolute threshold vs. % of last top-up.
- Stripe price-table for top-up bundles (e.g. \$10 = 1,000 coins).
- MediatR v11 vs. Mediator source-gen — defaulting to MediatR v11.
