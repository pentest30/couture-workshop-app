# Research: Atelier Couture Workshop App

**Feature**: 002-couture-workshop-app | **Date**: 2026-03-24

## R01 — Backend Boilerplate: Full Stack Hero (FSH)

**Decision**: Use [fullstackhero/dotnet-starter-kit](https://github.com/fullstackhero/dotnet-starter-kit) `develop` branch (v10.0.0-rc.1, .NET 10)

**Rationale**: FSH provides a production-ready modular monolith architecture with batteries included: multi-tenancy (Finbuckle), permission-based RBAC (Identity module), audit trail (Auditing module), background jobs (Hangfire), caching (HybridCache/Redis), OpenTelemetry observability, and structured error handling. This eliminates weeks of boilerplate work and provides proven patterns for exactly the features the couture app needs.

**What FSH provides out of the box**:
- Identity module: users, roles, permissions (Action + Resource matrix), JWT tokens
- Auditing module: automatic audit trail via EF Core interceptor
- Multi-tenancy module: tenant CRUD, per-tenant DB provisioning (single tenant for v1)
- BuildingBlocks: DDD primitives (BaseEntity, AuditableEntity, domain events), persistence base, validation pipeline, exception handling, caching, jobs, mailing, storage
- Source-generated Mediator 3.0 for CQRS (faster than MediatR)
- .NET Aspire AppHost for local dev orchestration
- FSH CLI for scaffolding new modules
- Architecture tests (NetArchTest) for enforcing module boundaries

**What we add on top**:
- 5 custom modules: Orders, Clients, Finance, Dashboard, Notifications
- SMS gateway adapter
- QuestPDF for receipt/report generation
- SignalR hub for real-time notification push
- Business day calculator (Algerian calendar)
- Custom permissions for 5 workshop roles

**Alternatives considered**:
- **CleanSliceTemplate (existing)**: Good but lacks built-in multi-tenancy, audit, identity. Would require building those from scratch.
- **Custom from scratch**: Maximum flexibility but 4-6 weeks of boilerplate work that FSH already solves.
- **ABP Framework**: Heavier, more opinionated, steeper learning curve, commercial license for some features.

## R02 — Frontend: React + shadcn/ui

**Decision**: React 19 SPA with Vite, shadcn/ui component library, Tailwind CSS 4, TanStack Router + Query

**Rationale**: shadcn/ui provides beautiful, accessible, copy-paste components built on Radix UI primitives. Unlike component libraries like MUI or Ant Design, shadcn/ui components are owned by the project (not a dependency), fully customizable, and lightweight. Tailwind CSS 4 enables rapid styling. TanStack Router provides type-safe routing. TanStack Query handles server state with caching, optimistic updates, and real-time invalidation (for notifications).

**Key shadcn/ui components for this project**:
- `DataTable` — order lists, client lists, payment tables (sortable, filterable, paginated)
- `Dialog` / `Sheet` — notification panel (slide-over), status change confirmation
- `Form` + `Input` + `Select` — 3-step order creation wizard
- `Badge` — status badges (colored), work type badges
- `Card` — KPI cards on dashboard
- `Tabs` — notification center (All/Unread/Critical)
- `Calendar` / `DatePicker` — delivery date selection
- `Command` — client search (combobox with debounced search)
- `Stepper` (custom) — order creation wizard steps
- `Toast` — success/error feedback
- `Chart` (Recharts integration) — dashboard analytics

**Alternatives considered**:
- **Next.js 15 SSR**: Adds server-side complexity not needed for a small workshop app behind auth. Vite SPA is simpler and faster to develop.
- **FSH Blazor UI (MudBlazor)**: Built into FSH but weaker ecosystem for charting, less mobile-responsive, and team prefers React.
- **Angular + PrimeNG**: Viable but heavier framework, slower iteration.

## R03 — Mobile: Flutter (Android + iOS)

**Decision**: Flutter 3.x compiled for both Android and iOS, included in v1 scope

**Rationale**: The spec requires mobile support for artisans in the field. Flutter provides a single Dart codebase for both platforms with native compilation. Key advantages for this project: Material 3 theming (consistent with shadcn palette), Riverpod for state management, Dio for HTTP, Hive for offline storage, and GoRouter for navigation. Flutter's offline-first patterns (local storage + sync queue) align with the P3 offline requirement.

**Mobile feature scope for v1**:
- Login + role-based navigation
- Order list (filtered by assigned artisan for Tailor/Embroiderer/Beader roles)
- Order detail + status transition actions
- Client search + view
- Notification center
- Record payment (Cashier/Manager)
- Offline queue for status changes and payments (sync on reconnect)

**Deferred to later**:
- Full dashboard with charts (complex charting in Flutter, web is primary)
- Full order creation wizard (web-first, mobile read/status-change focused)
- PDF receipt viewing (link to web or download)

**Alternatives considered**:
- **React Native**: Closer to React frontend but weaker offline patterns and slower native perf.
- **PWA only**: Insufficient for reliable offline and push notifications on iOS.
- **Kotlin Multiplatform**: Newer, smaller ecosystem, steeper learning curve.

## R04 — Database

**Decision**: PostgreSQL 16, single tenant for v1 (FSH multi-tenant capability available for future)

**Rationale**: FSH's Finbuckle integration supports per-tenant databases. For v1, we use a single tenant with one PostgreSQL database. This keeps deployment simple (single workshop) while preserving the option to go multi-tenant later. PostgreSQL provides: `unaccent` + `pg_trgm` extensions for diacritic-insensitive client search, JSONB for flexible measurement fields, and robust financial transaction support.

**Alternatives considered**:
- **SQL Server**: FSH supports it but PostgreSQL is free for on-premise deployment (critical for Algerian workshops without cloud).
- **Multi-tenant from day 1**: Adds complexity not needed for v1 (single workshop). FSH makes it easy to enable later.

## R05 — Authentication & Authorization

**Decision**: FSH built-in Identity module (JWT tokens + permission-based RBAC)

**Rationale**: FSH's Identity module provides everything needed: user management, role management, permission claims (Action + Resource), JWT token issuance/refresh, and tenant-aware auth. We map the 5 workshop roles to FSH permissions:

| FSH Permission | Manager | Tailor | Embroiderer | Beader | Cashier |
|----------------|---------|--------|-------------|--------|---------|
| Orders.Create | Yes | Yes | No | No | No |
| Orders.View | Yes | Own | Own | Own | Limited |
| Orders.Update | Yes | Limited | No | No | No |
| Orders.ChangeStatus | Yes | Limited | Limited | Limited | No |
| Orders.Deliver | Yes | No | No | No | Yes |
| Clients.Create | Yes | Yes | No | No | Yes |
| Clients.View | Yes | Yes | No | No | Yes |
| Finance.Record | Yes | No | No | No | Yes |
| Finance.View | Yes | No | No | No | Yes |
| Dashboard.View | Yes | No | No | No | Finance |
| Settings.Manage | Yes | No | No | No | No |
| Users.Manage | Yes | No | No | No | No |

**SMS OTP (2FA)**: Custom extension on FSH Identity using the SMS gateway adapter. Login flow: credentials -> JWT -> if 2FA enabled -> send OTP via SMS -> verify OTP -> final JWT.

**Alternatives considered**:
- **Cookie-based auth**: Simpler for web but JWT is better for mobile (Flutter) + web dual-client.
- **OpenIddict**: Overkill, FSH's built-in JWT is sufficient.

## R06 — CQRS: Mediator 3.0

**Decision**: Use FSH's built-in Mediator 3.0 (source-generated) instead of MediatR

**Rationale**: FSH `develop` branch replaced MediatR with Mediator 3.0, a source-generated alternative that is faster (no reflection) and has better compile-time safety. The API is nearly identical: `ICommand<TResponse>`, `IQuery<TResponse>`, `ICommandHandler<TCommand, TResponse>`. Pipeline behaviors work the same (validation, logging). Each feature is a vertical slice: Command/Query + Handler + Validator + Endpoint in one folder.

**Alternatives considered**:
- **MediatR 12**: FSH `main` branch uses it but `develop` has moved on. Sticking with FSH's choice avoids friction.

## R07 — Notification System

**Decision**: Hangfire background jobs (daily CRON + event-triggered) + SignalR for real-time in-app push + configurable SMS adapter

**Rationale**: FSH includes Hangfire with PostgreSQL storage and telemetry. We add:
1. **Daily job** (02:00): evaluates all active orders for N01 (overdue), N02 (24h), N03 (48h), N04 (stalled)
2. **Event-triggered**: domain events on status change trigger N05 (retouche), N06 (ready), N07 (assigned), N08 (unpaid delivery)
3. **SignalR hub**: pushes notifications to connected web/mobile clients in real-time (bell badge update)
4. **SMS adapter**: `ISmsGateway` with implementations for Twilio, Vonage, or local provider. Configurable time window (default 08:00-20:00). Delivery tracking (sent/delivered/failed).

FSH's InMemory eventing is used for cross-module events (e.g., Orders module publishes `OrderStatusChanged`, Notifications module subscribes).

**Alternatives considered**:
- **RabbitMQ**: FSH supports it but overkill for v1 per constitution (YAGNI). InMemory eventing is sufficient.
- **Polling**: Poor UX for notification badge, SignalR is simpler.

## R08 — PDF Generation

**Decision**: QuestPDF for receipt and report generation

**Rationale**: QuestPDF is a modern, fluent .NET library for creating PDFs programmatically. Handles receipt layout (workshop header, payment details, totals) and quarterly report (KPIs, charts as images, tables). Free community license for small-scale use. No external binary dependencies.

**Alternatives considered**:
- **iTextSharp**: Commercial license cost.
- **wkhtmltopdf**: External binary, hard to deploy in containers.

## R09 — Dashboard Charts

**Decision**: Recharts (React) for web dashboard, no charts in Flutter v1

**Rationale**: Recharts provides responsive, declarative React charts. Server-side data aggregation in PostgreSQL (GROUP BY quarter/month/status), sent as pre-computed JSON. Charts needed: monthly histogram (stacked bars), status donut, work type donut, revenue trend (line), delay by artisan (bar).

**Alternatives considered**:
- **Chart.js**: Also viable, Recharts has better React integration.
- **Flutter charts**: Deferred — dashboard is web-primary, mobile shows KPI cards only.

## R10 — Client Search (Diacritic-Insensitive)

**Decision**: PostgreSQL `unaccent` extension + `pg_trgm` trigram index

**Rationale**: Spec F06.4 requires partial, diacritic-insensitive search. PostgreSQL's `unaccent` strips diacritics, `pg_trgm` enables efficient partial matching. Combined with `GIN` index for <500ms response on ~2000 clients.

**Alternatives considered**:
- **Meilisearch**: External dependency, overkill for ~2000 clients.
- **Application-level normalization**: Slower, doesn't leverage indexes.

## R11 — Business Day Calculation

**Decision**: Custom utility accounting for Algerian weekend (Friday + Saturday) + configurable holidays

**Rationale**: Algeria observes Friday-Saturday weekends. A simple `BusinessDayCalculator` service computes business days excluding Fridays, Saturdays, and a configurable `Holiday` table. Used for: minimum delivery delay validation (RG02), delay calculation (RG06), notification threshold evaluation.

**Alternatives considered**:
- **NodaTime**: Heavy dependency for a simple calculation.

## R12 — Offline Strategy (Mobile)

**Decision**: Hive local storage + sync queue in Flutter, reconcile on reconnect

**Rationale**: Flutter mobile app stores pending operations (status changes, payments) in Hive when offline. On reconnect, a sync service sends queued operations with timestamps. Server applies operations in order, rejects conflicts (optimistic concurrency via row version). Conflicts are flagged in the notification center for manager resolution.

**Alternatives considered**:
- **CouchDB/PouchDB**: Web-only, doesn't help Flutter.
- **Isar**: Newer alternative to Hive, less mature.

## R13 — Deployment

**Decision**: Docker Compose for v1, .NET Aspire for local dev

**Rationale**: FSH includes Aspire AppHost for local development (orchestrates Postgres, Redis, OTLP collector). For production, a simple Docker Compose with: API container, PostgreSQL, Redis, and Nginx reverse proxy. On-premise deployable (workshop without cloud). Flutter builds via CI to APK (Android) and IPA (iOS).

**Alternatives considered**:
- **Kubernetes**: Overkill for a single workshop deployment.
- **Azure App Service**: Not suitable for on-premise requirement.
