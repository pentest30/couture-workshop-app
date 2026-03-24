# Quickstart: Atelier Couture Workshop App

**Feature**: 002-couture-workshop-app | **Date**: 2026-03-24

## Prerequisites

- .NET 10 SDK (`dotnet --version` >= 10.0.100)
- Node.js 22 LTS (`node --version` >= 22.0)
- Flutter 3.x (`flutter --version`)
- Docker & Docker Compose (for Postgres, Redis, and Aspire)
- Android Studio (for Android emulator) and/or Xcode (for iOS simulator)

## 1. Clone & Bootstrap FSH

```bash
# Clone FSH starter-kit (develop branch)
git clone -b develop https://github.com/fullstackhero/dotnet-starter-kit.git couture-backend
cd couture-backend

# Or use FSH CLI to scaffold
dotnet tool install --global FSH.CLI
fsh new couture --database postgres
```

## 2. Start Infrastructure (Aspire)

```bash
cd src/Couture.AppHost

# .NET Aspire starts: PostgreSQL 16 + Redis + OTLP collector
dotnet run
# Aspire dashboard: https://localhost:15888
# API: https://localhost:5001
# Postgres: localhost:5432
# Redis: localhost:6379
```

Alternatively, without Aspire:
```bash
# Docker Compose (Postgres + Redis)
docker compose up -d postgres redis
```

Required PostgreSQL extensions (run once on database):
```sql
CREATE EXTENSION IF NOT EXISTS "unaccent";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";
```

## 3. Backend Setup

```bash
cd src

# Restore packages
dotnet restore

# Apply migrations (all modules)
dotnet ef database update --project Couture.Api/Migrations.PostgreSQL --startup-project Couture.Api

# Run backend (with Hangfire dashboard)
dotnet run --project Couture.Api
# API:       https://localhost:5001
# Scalar UI: https://localhost:5001/scalar
# Hangfire:  https://localhost:5001/hangfire
```

### Default seed data (via FSH tenant provisioning)
- **Root tenant**: `couture-workshop` (auto-provisioned)
- **Admin user**: username `admin@couture.local`, password `Admin123!`, role: Manager
- **Permissions**: All workshop permissions seeded for Manager role
- **Measurement fields**: 10 default fields (Tour de poitrine, Tour de taille, etc.)
- **Notification config**: N01-N08 enabled with default thresholds
- **Workshop settings**: max 10 orders/tailor, 8h session, SMS window 08:00-20:00

## 4. Frontend Setup

```bash
cd frontend

# Install dependencies
npm install

# Initialize shadcn/ui (if not already done)
npx shadcn@latest init

# Set API URL
cp .env.example .env.local
# Edit .env.local:
# VITE_API_URL=https://localhost:5001
# VITE_SIGNALR_URL=https://localhost:5001/hubs/notifications

# Run dev server
npm run dev
# App: http://localhost:5173
```

### Install shadcn/ui components used
```bash
npx shadcn@latest add button card badge input select dialog sheet \
  form label textarea tabs table command calendar toast separator \
  avatar dropdown-menu popover scroll-area skeleton switch tooltip
```

## 5. Mobile Setup (Flutter)

```bash
cd mobile

# Get dependencies
flutter pub get

# Configure API URL
# Edit lib/core/config.dart:
# static const apiUrl = 'https://10.0.2.2:5001'; // Android emulator
# static const apiUrl = 'https://localhost:5001'; // iOS simulator

# Run on Android
flutter run -d android

# Run on iOS
flutter run -d ios

# Build release APK
flutter build apk --release

# Build release iOS (requires Xcode + signing)
flutter build ios --release
```

## 6. Run Tests

```bash
# Backend: all tests
dotnet test src/Tests/

# Backend: specific module
dotnet test src/Tests/Orders.Tests
dotnet test src/Tests/Architecture.Tests

# Frontend: unit + component tests
cd frontend && npm test

# Frontend: E2E
cd frontend && npx playwright test

# Mobile: Flutter tests
cd mobile && flutter test
```

## 7. Key URLs (Development)

| Service | URL |
|---------|-----|
| Aspire Dashboard | https://localhost:15888 |
| Backend API | https://localhost:5001 |
| Scalar (OpenAPI docs) | https://localhost:5001/scalar |
| Hangfire Dashboard | https://localhost:5001/hangfire |
| Frontend App | http://localhost:5173 |
| PostgreSQL | localhost:5432 |
| Redis | localhost:6379 |

## 8. Environment Configuration

### Backend (`appsettings.Development.json`)
```json
{
  "DatabaseOptions": {
    "Provider": "postgresql",
    "ConnectionString": "Host=localhost;Port=5432;Database=couture_dev;Username=postgres;Password=postgres"
  },
  "CacheOptions": {
    "RedisUrl": "localhost:6379"
  },
  "JwtOptions": {
    "Key": "dev-secret-key-min-32-characters-long!",
    "TokenExpirationInMinutes": 480,
    "RefreshTokenExpirationInDays": 7
  },
  "HangfireOptions": {
    "Storage": "postgresql",
    "DashboardEnabled": true
  },
  "SmsOptions": {
    "Provider": "mock",
    "WindowStart": "08:00",
    "WindowEnd": "20:00"
  },
  "StorageOptions": {
    "Provider": "local",
    "BasePath": "./uploads"
  },
  "SignalROptions": {
    "Enabled": true
  }
}
```

### Frontend (`.env.local`)
```
VITE_API_URL=https://localhost:5001
VITE_SIGNALR_URL=https://localhost:5001/hubs/notifications
```

### Mobile (`lib/core/config.dart`)
```dart
class AppConfig {
  static const apiUrl = 'https://10.0.2.2:5001'; // Android emulator
  // static const apiUrl = 'https://localhost:5001'; // iOS simulator
}
```

## 9. Project Structure (Quick Reference)

```
src/
  BuildingBlocks/        → FSH reusable libs (Core, Persistence, Web, Jobs, etc.)
  Modules/
    Identity/            → FSH: users, roles, permissions, JWT auth
    Auditing/            → FSH: automatic audit trail
    Multitenancy/        → FSH: tenant management
    Orders/              → Custom: order lifecycle, statuses, work types
    Clients/             → Custom: client registry, measurements
    Finance/             → Custom: payments, receipts (QuestPDF)
    Dashboard/           → Custom: KPIs, charts, exports
    Notifications/       → Custom: alerts, SMS, SignalR hub, Hangfire jobs
  Couture.AppHost/       → .NET Aspire orchestrator
  Couture.Api/           → Web API host
  Tests/                 → xUnit + NetArchTest per module

frontend/
  src/routes/            → TanStack Router pages
  src/components/ui/     → shadcn/ui components
  src/components/*/      → Domain-specific components
  src/hooks/             → TanStack Query hooks
  src/services/          → API client + SignalR

mobile/
  lib/features/          → Flutter screens per module
  lib/core/              → Auth, network, offline storage
  lib/shared/            → Reusable widgets
```
