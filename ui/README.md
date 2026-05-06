# Email Platform — UI

This folder will contain the web frontends for the Email Platform. See the root [README.md](../README.md) for product context and `../api/README.md` for the backend it talks to.

## Status

**Not yet scaffolded.** The backend (`/api`) is being built first; the UI starts after the MVP backend slice is runnable. The frontend framework is **TBD** — to be picked when work on this folder begins.

## Planned applications

Two distinct applications will live here, deployed and authenticated separately:

| App | Audience | Auth |
|---|---|---|
| `tenant-ui` | Tenant admins and team members | JWT issued by `Identity.Api` (tenant user pool) |
| `admin-ui` | Platform operators | Separate admin user pool, separate hostname, separate IdP realm — never shares auth with the tenant app |

Keeping them separate is a deliberate blast-radius choice: a tenant-pool credential leak must not give access to admin tooling.

## Planned features

### Tenant UI

- Send composer: pick template, pick audience, schedule (`scheduledAt`), recurrence rule.
- Audience and recipient management — CRUD, CSV import, suppression view.
- Template authoring with merge-field preview (templates are rendered server-side with [Scriban](https://github.com/scriban/scriban); UI just edits and previews).
- Per-locale template variants (e.g. `welcome:en-US`, `welcome:fr-FR`) — UI picks the right variant for each recipient based on `recipient.locale`.
- Campaign dashboard — live status of running campaigns, real-time delivery events over **SignalR**.
- Settings: sender configuration (paste a verified SendGrid sender ID, or use the shared sender), API keys, outbound webhook subscriptions, low-balance threshold.
- Billing: top-up via Stripe Checkout, coin balance, transaction history, low-balance warnings.
- Multi-language UI: English and at least one additional language at launch (i18n framework choice TBD).

### Admin UI

- Tenant directory: list, create, suspend, inspect.
- Per-tenant metrics: send volume, deliverability, complaint rate, coin balance.
- Platform-wide metrics dashboards (visualisations sourced from Grafana panels embedded or replicated).
- Configuration surface: rate limits, default coin-conversion rate, alert thresholds.
- Logs and audit trail.

## Talking to the API

- **REST** for the majority of operations (CRUD, send requests, campaign management, top-ups).
- **WebSockets via SignalR** for real-time surfaces only — campaign progress, delivery events, low-balance alerts.
- Tenant requests go through the YARP gateway at the API tier. Admin requests hit `Admin.Api` on a separate hostname.

## Running locally

> Not yet runnable. This README will be updated once the framework is picked and the apps are scaffolded.

## Open decisions

- Frontend framework (React, Vue, Svelte, Blazor, Angular).
- One repo with two apps (monorepo with shared design system) vs. two top-level apps with no shared code.
- Component library / design system.
- i18n library (depends on framework choice).
- Charting library for dashboards.
- Build/dev tooling.
