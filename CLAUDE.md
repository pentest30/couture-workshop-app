# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Couture Workshop App — a management system for an Algerian couture atelier (devis → livraison). Tracks orders, clients, measurements, payments, notifications, and dashboard KPIs. The business domain is in French (commande, statut, acompte, solde, etc.).

## Architecture

**Backend** (`src/`): ASP.NET Core 10 modular monolith (.NET 10 / C# 13). One solution (`src/Couture.sln`) with a thin API host and six domain modules:

- `src/Api/` — Minimal API host. Endpoint registration in `Endpoints/` files, all wired in `Program.cs`. Auto-migrates and seeds in Development.
- `src/Modules/{Module}/{Module}/` — Each module follows: `Domain/` (entities, value objects, SmartEnums), `Features/` (CQRS handlers via Mediator source generator), `Persistence/` (EF Core DbContext + configurations), `Migrations/`.
- `src/Modules/{Module}/{Module}.Contracts/` — Strongly-typed IDs (Guid wrappers) and DTOs shared across modules.
- `src/SharedKernel/` — `AggregateRoot`, `AuditableEntity`, `IDomainEvent` base types.

Modules: **Identity** (auth/JWT/roles), **Orders** (order lifecycle, status transitions, work types), **Clients** (client profiles, measurements), **Finance** (payments, receipts/PDF), **Dashboard** (KPIs, reports, Excel export), **Notifications** (in-app + SMS via SignalR hub, background jobs).

Each module has its own `DbContext` and migrations — they share a single PostgreSQL database but are logically isolated. Central package versioning via `src/Directory.Packages.props`.

**Mobile** (`mobile/`): Flutter 3.x app (Dart 3.6+). Uses Riverpod for state, GoRouter for navigation, Dio for HTTP. Feature-based structure under `mobile/lib/features/` (auth, orders, clients, dashboard, notifications, new_order). Shared code in `mobile/lib/core/` (API client, theme, helpers, widgets).

**Database**: PostgreSQL 16 via Docker (`docker-compose.yml` at root). Connection: `localhost:5432`, db `couture_dev`, user/pass `postgres/postgres`.

## Commands

### Backend (.NET)

```bash
# Start PostgreSQL
docker compose up -d

# Run API (from src/Api/)
dotnet run --project src/Api

# Run all tests
dotnet test src/Couture.sln

# Run a single test project
dotnet test src/Tests/Domain.Tests

# Run a specific test by name
dotnet test src/Tests/Domain.Tests --filter "OrderStatusTransitionTests"

# Build solution
dotnet build src/Couture.sln

# Add migration (example for Orders module)
dotnet ef migrations add MigrationName --project src/Modules/Orders/Orders --startup-project src/Api
```

### Mobile (Flutter)

```bash
# From mobile/ directory:
cd mobile
flutter pub get
flutter run              # run on connected device/emulator
flutter analyze          # lint
flutter test             # run tests
```

## Key Domain Concepts

- **WorkType**: Simple, Brodé (embroidered), Perlé (beaded), Mixte (both) — each has different workflow rules
- **Order status matrix**: Status transitions are enforced server-side in `Orders/Domain/OrderStatus.cs`. The allowed transitions follow the functional spec (SPECS_FONCTIONNELLES.md §F02)
- **Financial rules**: Acompte (deposit) → solde (balance). Delivery (LIVRÉE) requires full payment unless manager override. Enforced in domain logic.
- **Roles**: Gérant (manager), Couturier(ère), Brodeur/Perleur, Caissier — role-based access is a core requirement

## Key Files

- `constitution.md` — Architectural principles and non-negotiable rules (domain-first, role-based access, test-driven)
- `SPECS_FONCTIONNELLES.md` — Full functional specification (authoritative for business rules)
- `specs/plan.md` — Implementation plan and tech decisions
- `src/Api/Program.cs` — DI registration, middleware, seed data
- `mobile/lib/core/api/api_client.dart` — Mobile API client (base URL is a LAN IP, adjust for your network)

## Test Framework

xUnit + FluentAssertions. Tests mirror module structure under `src/Tests/`. Domain tests cover status transitions, payment rules, and business invariants. Integration tests cover full order lifecycle flows. EF InMemory provider used for handler tests.
