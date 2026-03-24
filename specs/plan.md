# Implementation Plan: Atelier Couture Workshop App

**Branch**: `002-couture-workshop-app` | **Date**: 2026-03-24 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-couture-workshop-app/spec.md`

## Summary

Couture workshop management application digitalizing the full order lifecycle — from client intake and order creation through artisan assignment, status tracking (Simple/Embroidered/Beaded/Mixed workflows), to delivery and financial settlement. Built as a **Modular Monolith** on **Full Stack Hero (FSH) dotnet-starter-kit** (develop branch, .NET 10) with PostgreSQL, a **React 19 + shadcn/ui** SPA frontend, and a **Flutter** mobile app compiled for both Android and iOS. The system provides role-based access (Manager, Tailor, Embroiderer, Beader, Cashier), proactive notifications (in-app via SignalR + SMS), quarterly analytics dashboard with KPIs, PDF receipt generation, and offline-capable mobile operation.

## Technical Context

**Language/Version**: C# 13 / .NET 10.0 (backend), TypeScript 5.x (frontend), Dart 3.x (mobile)
**Boilerplate**: [Full Stack Hero dotnet-starter-kit](https://github.com/fullstackhero/dotnet-starter-kit) `develop` branch (v10.0.0-rc.1)
**Primary Dependencies**:
- *Backend*: ASP.NET Core 10, EF Core 10, Mediator 3.0 (source-generated), FluentValidation 12, Finbuckle.MultiTenant 10, Hangfire 1.8, Mapster, Serilog, OpenTelemetry, Scalar (OpenAPI)
- *Frontend*: React 19, Vite, shadcn/ui, Tailwind CSS 4, TanStack Query, TanStack Router, Recharts
- *Mobile*: Flutter 3.x, Riverpod, Dio, Hive (offline storage)
**Storage**: PostgreSQL 16 (per-tenant via FSH, single tenant for v1) + Redis (HybridCache)
**Testing**: xUnit, NSubstitute, Shouldly, AutoFixture, TestContainers, NetArchTest (backend); Vitest, React Testing Library, Playwright (frontend); Flutter test (mobile)
**Target Platform**: Linux containers (Docker) via .NET Aspire for backend, Web (modern browsers) + Android + iOS (Flutter)
**Project Type**: Web API (FSH modular monolith) + React SPA + Flutter mobile app
**Performance Goals**: <2s dashboard load, <500ms client search, <3s PDF generation, 20 concurrent users
**Constraints**: Algeria connectivity (3G/4G), French language (fr-DZ), DZD currency, Algerian weekend (Fri/Sat), SMS delivery within configurable hours, offline-capable mobile
**Scale/Scope**: ~20 concurrent users per workshop, ~5000 orders/year, ~2000 clients, 5 custom modules + 3 FSH built-in modules, ~40 API endpoints, ~15 web screens, ~12 mobile screens

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Domain-First & Traceability | PASS | Rich domain model in dedicated modules (Orders, Clients, Finance). FSH's `AuditableEntity` base class + `AuditInterceptor` provides automatic audit trail. Status transitions validated server-side in domain aggregate. Financial calculations enforced in Order domain. |
| II. Role-Based Access (NON-NEGOTIABLE) | PASS | FSH's built-in Identity module provides permission-based auth (Action + Resource matrix). 5 workshop roles mapped to FSH permissions. `RequiredPermissionAuthorizationHandler` enforces on every endpoint. |
| III. Test-Driven Quality | PASS | FSH includes Architecture.Tests scaffold. Unit tests for status transitions, financial calculations, work type rules. Integration tests for order lifecycle, dashboard aggregation. Edge case tests for alteration with reason, delivery with unpaid balance, duplicate client detection. |
| IV. Explicit Errors & UX Clarity | PASS | FSH's built-in exception handling middleware returns structured ProblemDetails. FluentValidation pipeline behavior for all commands. Explicit domain exceptions for invalid transitions and business rule violations. |
| V. Simplicity & YAGNI (v1) | PASS | FSH modular monolith (no microservices). InMemory eventing (no RabbitMQ for v1). Hangfire for notification scheduling. Single tenant for v1 (multi-tenant capability available via FSH if needed later). Excel import (M09) out of scope. |

All gates pass. No violations to justify.

## Project Structure

### Documentation (this feature)

```text
specs/002-couture-workshop-app/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (API contracts)
│   ├── orders-api.md
│   ├── clients-api.md
│   ├── dashboard-api.md
│   ├── notifications-api.md
│   ├── users-api.md
│   └── finance-api.md
├── checklists/
│   └── requirements.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── BuildingBlocks/                       # FSH reusable libraries (from starter-kit)
│   ├── Caching/                          # HybridCache (Redis-backed)
│   ├── Core/                             # DDD primitives: BaseEntity, AuditableEntity, domain events
│   ├── Eventing/                         # InMemory integration events (RabbitMQ disabled for v1)
│   ├── Eventing.Abstractions/            # IEventBus, IIntegrationEvent contracts
│   ├── Jobs/                             # Hangfire wrappers + telemetry
│   ├── Mailing/                          # SMTP/SendGrid abstractions
│   ├── Persistence/                      # EF Core DbContext base, interceptors, specifications
│   ├── Shared/                           # Shared DTOs (Auditing, Identity, Multitenancy, Storage)
│   ├── Storage/                          # Local file storage (photos, PDFs)
│   └── Web/                              # ASP.NET Core host: Auth, CORS, Exceptions, OpenApi, Validation
│
├── Modules/
│   ├── Auditing/                         # FSH built-in — audit trail tracking
│   │   ├── Auditing.Contracts/
│   │   └── Auditing/
│   │
│   ├── Identity/                         # FSH built-in — users, roles, tokens, permissions
│   │   ├── Identity.Contracts/
│   │   └── Identity/
│   │
│   ├── Multitenancy/                     # FSH built-in — tenant management (single tenant v1)
│   │   ├── Multitenancy.Contracts/
│   │   └── Multitenancy/
│   │
│   ├── Orders/                           # CUSTOM — Order lifecycle, status transitions, work types
│   │   ├── Orders.Contracts/             # Shared interfaces, DTOs, events
│   │   │   ├── Commands/                 # CreateOrder, ChangeStatus, UpdateOrder
│   │   │   ├── Queries/                  # GetOrder, ListOrders, GetTimeline
│   │   │   ├── Events/                   # OrderCreated, StatusChanged, OrderDelivered
│   │   │   └── Dtos/                     # OrderDto, OrderSummaryDto, TimelineEntryDto
│   │   └── Orders/                       # Runtime implementation
│   │       ├── Domain/                   # Order aggregate, StatusTransition, WorkType SmartEnum
│   │       │   ├── Order.cs              # Aggregate root with status machine
│   │       │   ├── OrderStatus.cs        # SmartEnum (8 statuses)
│   │       │   ├── WorkType.cs           # SmartEnum (Simple/Brodé/Perlé/Mixte)
│   │       │   ├── StatusTransition.cs   # Value object
│   │       │   └── OrderPhoto.cs         # Entity
│   │       ├── Features/                 # Vertical slices (command/query + handler + endpoint)
│   │       │   ├── CreateOrder/
│   │       │   ├── UpdateOrder/
│   │       │   ├── ChangeStatus/
│   │       │   ├── GetOrder/
│   │       │   ├── ListOrders/
│   │       │   ├── GetTimeline/
│   │       │   └── UploadPhotos/
│   │       ├── Persistence/              # EF config, DbContext, migrations
│   │       └── Extensions.cs             # Module DI registration
│   │
│   ├── Clients/                          # CUSTOM — Client registry, measurements
│   │   ├── Clients.Contracts/
│   │   │   ├── Commands/                 # CreateClient, UpdateClient, RecordMeasurements
│   │   │   ├── Queries/                  # GetClient, SearchClients, GetMeasurementHistory
│   │   │   └── Dtos/
│   │   └── Clients/
│   │       ├── Domain/                   # Client aggregate, MeasurementField, ClientMeasurement
│   │       ├── Features/                 # Vertical slices
│   │       │   ├── CreateClient/
│   │       │   ├── UpdateClient/
│   │       │   ├── SearchClients/
│   │       │   ├── GetClient/
│   │       │   ├── RecordMeasurements/
│   │       │   ├── GetMeasurementHistory/
│   │       │   └── ManageMeasurementFields/
│   │       └── Persistence/
│   │
│   ├── Finance/                          # CUSTOM — Payments, receipts, financial dashboard
│   │   ├── Finance.Contracts/
│   │   │   ├── Commands/                 # RecordPayment
│   │   │   ├── Queries/                  # GetFinancialSummary, GetPayments
│   │   │   └── Events/                   # PaymentRecorded
│   │   └── Finance/
│   │       ├── Domain/                   # Payment, Receipt, PaymentMethod SmartEnum
│   │       ├── Features/
│   │       │   ├── RecordPayment/
│   │       │   ├── GetPayments/
│   │       │   ├── GetFinancialSummary/
│   │       │   └── DownloadReceipt/
│   │       ├── Pdf/                      # QuestPDF receipt/report generation
│   │       └── Persistence/
│   │
│   ├── Dashboard/                        # CUSTOM — Quarterly KPIs, charts, exports
│   │   ├── Dashboard.Contracts/
│   │   │   ├── Queries/                  # GetKPIs, GetChartData
│   │   │   └── Dtos/
│   │   └── Dashboard/
│   │       ├── Features/
│   │       │   ├── GetQuarterlyKPIs/
│   │       │   ├── GetMonthlyHistogram/
│   │       │   ├── GetStatusDistribution/
│   │       │   ├── GetRevenueTrend/
│   │       │   ├── GetDelayByArtisan/
│   │       │   └── ExportReport/
│   │       └── Services/                 # BusinessDayCalculator, ExportService
│   │
│   └── Notifications/                    # CUSTOM — In-app + SMS alerts, notification config
│       ├── Notifications.Contracts/
│       │   ├── Commands/                 # MarkRead, UpdateConfig
│       │   ├── Queries/                  # ListNotifications, GetUnreadCount
│       │   └── Events/                   # NotificationCreated
│       └── Notifications/
│           ├── Domain/                   # Notification, NotificationType SmartEnum, NotificationConfig
│           ├── Features/
│           │   ├── ListNotifications/
│           │   ├── MarkRead/
│           │   ├── ConfigureAlerts/
│           │   ├── GetSmsLogs/
│           │   └── TestSms/
│           ├── Jobs/                     # Hangfire: EvaluateOverdueOrders, EvaluateStalledOrders
│           ├── Sms/                      # ISmsGateway adapter (Twilio/Vonage/local)
│           ├── Hub/                      # SignalR NotificationHub
│           └── Persistence/
│
├── Couture.AppHost/                      # .NET Aspire orchestrator (Postgres + Redis + OTLP)
├── Couture.Api/                          # Web API host (wires all modules)
│   └── Migrations.PostgreSQL/            # Consolidated EF Core migrations
│
└── Tests/
    ├── Architecture.Tests/               # NetArchTest: module boundaries, naming, layering
    ├── Orders.Tests/                     # Order domain + feature tests
    ├── Clients.Tests/                    # Client domain + feature tests
    ├── Finance.Tests/                    # Payment/receipt tests
    ├── Dashboard.Tests/                  # KPI aggregation tests
    └── Notifications.Tests/              # Alert evaluation, SMS delivery tests

frontend/                                 # React SPA with shadcn/ui
├── public/
├── src/
│   ├── routes/                           # TanStack Router pages
│   │   ├── _layout.tsx                   # Root layout (sidebar, navbar, notification bell)
│   │   ├── _auth.tsx                     # Auth layout guard
│   │   ├── dashboard/
│   │   │   └── index.tsx                 # Quarterly dashboard with KPI cards + charts
│   │   ├── orders/
│   │   │   ├── index.tsx                 # Order list with filters
│   │   │   ├── $orderId.tsx              # Order detail + timeline
│   │   │   └── new.tsx                   # 3-step order creation wizard
│   │   ├── clients/
│   │   │   ├── index.tsx                 # Client list + search
│   │   │   └── $clientId.tsx             # Client detail + measurements
│   │   ├── finance/
│   │   │   └── index.tsx                 # Financial dashboard + payment tracking
│   │   ├── notifications/
│   │   │   └── index.tsx                 # Full notification center
│   │   ├── admin/
│   │   │   ├── users.tsx                 # User management
│   │   │   ├── settings.tsx              # Workshop settings, holidays
│   │   │   └── notifications.tsx         # Notification configuration
│   │   └── auth/
│   │       ├── login.tsx                 # Login + OTP
│   │       └── index.tsx                 # Redirect
│   ├── components/
│   │   ├── ui/                           # shadcn/ui components (auto-generated)
│   │   ├── orders/                       # StatusBadge, WorkTypeBadge, Timeline, OrderForm
│   │   ├── dashboard/                    # KPICard, QuarterSelector, Charts
│   │   ├── clients/                      # MeasurementGrid, ClientSearch, ClientStats
│   │   ├── notifications/                # NotificationBell, NotificationPanel
│   │   └── layout/                       # Sidebar, Navbar, RoleGuard
│   ├── hooks/                            # TanStack Query hooks per module
│   │   ├── use-orders.ts
│   │   ├── use-clients.ts
│   │   ├── use-dashboard.ts
│   │   ├── use-notifications.ts
│   │   ├── use-finance.ts
│   │   └── use-auth.ts
│   ├── services/                         # API client (Axios/fetch), SignalR client
│   ├── lib/                              # Utils: DZD formatter, date formatter (JJ/MM/AAAA), business day calc
│   └── styles/                           # Tailwind config, global styles
├── tests/
│   ├── components/                       # Vitest + React Testing Library
│   └── e2e/                              # Playwright
├── index.html
├── vite.config.ts
├── tailwind.config.ts
├── components.json                       # shadcn/ui config
├── tsconfig.json
└── package.json

mobile/                                   # Flutter app (Android + iOS)
├── lib/
│   ├── main.dart
│   ├── app/                              # App-level config, routing, theme
│   │   ├── router.dart                   # GoRouter routes
│   │   └── theme.dart                    # Material 3 theme matching shadcn palette
│   ├── core/
│   │   ├── auth/                         # JWT token management, login
│   │   ├── network/                      # Dio HTTP client, interceptors
│   │   ├── storage/                      # Hive local storage (offline)
│   │   ├── sync/                         # Offline queue + sync on reconnect
│   │   └── providers/                    # Riverpod providers
│   ├── features/
│   │   ├── dashboard/                    # KPI cards, quarter selector (read-only)
│   │   ├── orders/
│   │   │   ├── order_list_screen.dart    # Filterable list
│   │   │   ├── order_detail_screen.dart  # Detail + timeline + status actions
│   │   │   └── create_order_screen.dart  # 3-step wizard
│   │   ├── clients/
│   │   │   ├── client_list_screen.dart   # Search
│   │   │   └── client_detail_screen.dart # Measurements
│   │   ├── notifications/
│   │   │   └── notification_screen.dart  # Bell + list
│   │   └── finance/
│   │       └── payment_screen.dart       # Record payment
│   └── shared/                           # Widgets: StatusBadge, WorkTypeBadge, etc.
├── test/
├── android/
├── ios/
├── pubspec.yaml
└── analysis_options.yaml
```

**Structure Decision**: FSH modular monolith backend with 5 custom domain modules (Orders, Clients, Finance, Dashboard, Notifications) + 3 FSH built-in modules (Identity, Auditing, Multitenancy). React SPA with shadcn/ui for web. Flutter for mobile (Android + iOS) with offline support. FSH's Aspire AppHost orchestrates local dev (Postgres + Redis). No Blazor UI from FSH — replaced by React + shadcn/ui frontend.

## Complexity Tracking

> One justified deviation from FSH defaults.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| React SPA instead of FSH Blazor | Workshop staff are mobile-first; React + shadcn/ui provides better responsive UX, richer charting ecosystem (Recharts), and alignment with Flutter for shared design tokens | Blazor WebAssembly has weaker offline story and limited charting libraries |
