# Email Platform

Multi-tenant SaaS for sending bulk promotional email at scale, with tenant-managed audiences, recurring campaigns, prepaid usage-based billing, and full delivery-event visibility.

Tenants send via REST API or web UI, recipients are organised into audiences they manage in our database, and emails are rendered server-side from versioned templates with per-locale variants. SendGrid handles actual delivery; the platform sits in front of it adding tenancy, scheduling, suppressions, billing, compliance, and observability.

## What it does

- **Bulk promotional sends** addressed by `audienceId`, with recipient dedupe and suppression-list enforcement applied before billing.
- **Transactional sends** (e.g. password resets) via ad-hoc recipient arrays, bypassing suppression and unsubscribe.
- **Templates** in our DB, rendered server-side with [Scriban](https://github.com/scriban/scriban). Per-locale variants resolved against `recipient.locale`.
- **Scheduled and recurring campaigns** — every send carries `scheduledAt` (null defaults to now); recurring campaigns resolve their audience at fire time.
- **Prepaid coin ledger.** Tenants top up via Stripe; every send debits coins by size + count + factors. Atomic pre-flight check rejects sends that exceed balance (no partial campaigns). 75% low-balance alerts.
- **Delivery events** ingested from SendGrid webhooks, fanned out to outbound tenant webhooks (HMAC-signed, retried), to the tenant UI in real-time over SignalR, and to the admin dashboard.
- **Multi-tenant isolation** enforced at the data layer: every document carries `tenantId`, every query filters on it, and a dedicated cross-tenant leak test suite asserts no endpoint can cross tenants.
- **Compliance baked in:** mandatory unsubscribe on promotional sends, automatic suppression on hard bounce / spam complaint / unsubscribe, per-tenant rate limits to protect shared sender reputation.
- **Regional-cell architecture.** Every tenant has a `homeRegion`; v1 ships a single AWS Cape Town cell, additional cells (EU, others) ship later without code changes.

## Tech stack

| Concern | Choice |
|---|---|
| Backend | .NET 8 LTS, ASP.NET Core, MongoDB.Driver, MassTransit v8 (RabbitMQ), MediatR v11, FluentValidation, Polly v8, Scriban, Serilog |
| Database | MongoDB 7 (per-service databases, shared collections with `tenantId` discriminator) |
| Messaging | RabbitMQ |
| Email provider | SendGrid (sandbox in dev) — pluggable `IEmailProvider` |
| Payments | Stripe — pluggable `IPaymentProvider` (M-Pesa to follow) |
| Auth | JWT for tenant users, API keys for tenant programmatic access, separate user pool for platform admins |
| Real-time | SignalR (UI) and outbound webhooks (tenant integrations) |
| Observability | OpenTelemetry → OTel Collector → Loki / Tempo / Prometheus / Grafana |
| Frontend | TBD (see `ui/README.md`) |

## High-level architecture

```
                     ┌──────────────┐         ┌──────────────┐
   Tenant API/UI ───▶│   Gateway    │────────▶│   Identity   │
                     │   (YARP)     │         └──────────────┘
                     └──────┬───────┘
                            │
            ┌───────────────┼───────────────┬───────────────┐
            ▼               ▼               ▼               ▼
       ┌─────────┐   ┌──────────────┐  ┌─────────┐   ┌───────────────┐
       │ Tenants │   │ EmailSending │  │ Billing │   │ Notifications │
       │  .Api   │   │     .Api     │  │  .Api   │   │     .Api      │
       └────┬────┘   └──────┬───────┘  └────┬────┘   └───────┬───────┘
            │               │               │                │
            │               ▼               │                ├──── SignalR ──▶ UI
            │      ┌───────────────────┐    │                │
            │      │ EmailSending      │    │                └──── HTTPS ────▶ Tenant webhooks
            │      │    .Worker        │    │
            │      │ (queue consumer + │    │
            │      │  campaign sched.) │    │
            │      └────────┬──────────┘    │
            │               │               │
            │               ▼               │
            │         ┌──────────┐          │
            │         │ SendGrid │          │
            │         └────┬─────┘          │
            │              │ webhooks       │
            │              ▼                │
            │     ┌────────────────┐        │
            │     │ DeliveryEvents │────────┘
            │     │     .Api       │   integration events
            │     └────────────────┘
            │
            ▼
       ┌─────────────────────────────────────┐
       │       MongoDB · RabbitMQ            │
       └─────────────────────────────────────┘

  Admin.Api runs on a separate hostname with its own user pool — no shared auth with tenants.
```

## Repo layout

```
/
├── api/      # .NET 8 microservices solution (see api/README.md)
├── ui/       # tenant UI + admin UI (see ui/README.md)
├── docker-compose.yml   # shared infra: MongoDB, RabbitMQ
├── .env
├── LICENSE
└── README.md
```

## Status

Currently in **Phase 0 — foundation**. The `/api` solution skeleton, BuildingBlocks projects, and the telemetry stack in `docker-compose.dev.yml` are next. See `api/README.md` for the full phased build order.

## Local development

Shared infrastructure is up via the root `docker-compose.yml`:

```bash
cp .env.example .env   # edit secrets
docker compose up -d
```

This brings up MongoDB and RabbitMQ. Service-level setup lives in `api/README.md` and `ui/README.md`.

## License

See [LICENSE](LICENSE).
