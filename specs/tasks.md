# Tasks: Atelier Couture Workshop App

**Input**: Design documents from `/specs/002-couture-workshop-app/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Not explicitly requested — test tasks omitted. Add with TDD approach if desired.

**Organization**: Tasks grouped by user story (10 stories: US1-US10) to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1-US10)
- Exact file paths from plan.md structure

---

## Phase 1: Setup (Project Initialization)

**Purpose**: Bootstrap FSH backend, React frontend, and Flutter mobile projects

- [ ] T001 Clone FSH dotnet-starter-kit develop branch and rename to project structure in src/
- [ ] T002 Configure Aspire AppHost with PostgreSQL 16 + Redis in src/Couture.AppHost/Program.cs
- [ ] T003 [P] Remove FSH Blazor UI projects (Blazor.UI, Playground.Blazor) — replaced by React frontend
- [ ] T004 [P] Remove FSH sample module (Playground/Catalog) — replaced by custom modules
- [ ] T005 [P] Scaffold React project with Vite + TypeScript in frontend/ (npm create vite@latest)
- [ ] T006 [P] Scaffold Flutter project for Android + iOS in mobile/ (flutter create couture_mobile)
- [ ] T007 Install shadcn/ui + Tailwind CSS 4 in frontend/ (npx shadcn@latest init, configure components.json)
- [ ] T008 Install shadcn/ui base components: button, card, badge, input, select, dialog, sheet, form, label, textarea, tabs, table, command, calendar, toast, separator, avatar, dropdown-menu, popover, scroll-area, skeleton, switch, tooltip in frontend/
- [ ] T009 [P] Install frontend dependencies: TanStack Router, TanStack Query, Recharts, @microsoft/signalr, date-fns in frontend/package.json
- [ ] T010 [P] Install Flutter dependencies: riverpod, dio, hive, go_router, intl, connectivity_plus in mobile/pubspec.yaml
- [ ] T011 Configure TanStack Router with file-based routing in frontend/src/routes/
- [ ] T012 [P] Configure API client service with JWT interceptor in frontend/src/services/api-client.ts
- [ ] T013 [P] Configure Dio HTTP client with JWT interceptor in mobile/lib/core/network/api_client.dart
- [ ] T014 [P] Create shared utility: DZD currency formatter in frontend/src/lib/formatters.ts
- [ ] T015 [P] Create shared utility: date formatter (JJ/MM/AAAA) in frontend/src/lib/formatters.ts
- [ ] T016 [P] Create Flutter shared utility: DZD formatter + date formatter in mobile/lib/core/utils/formatters.dart

**Checkpoint**: Three projects bootstrapped — backend (FSH), frontend (React + shadcn), mobile (Flutter) all compile and run

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story

**WARNING**: No user story work can begin until this phase is complete

- [ ] T017 Create Orders module scaffold: Orders.Contracts/ and Orders/ projects with Extensions.cs in src/Modules/Orders/
- [ ] T018 [P] Create Clients module scaffold: Clients.Contracts/ and Clients/ projects with Extensions.cs in src/Modules/Clients/
- [ ] T019 [P] Create Finance module scaffold: Finance.Contracts/ and Finance/ projects with Extensions.cs in src/Modules/Finance/
- [ ] T020 [P] Create Dashboard module scaffold: Dashboard.Contracts/ and Dashboard/ projects with Extensions.cs in src/Modules/Dashboard/
- [ ] T021 [P] Create Notifications module scaffold: Notifications.Contracts/ and Notifications/ projects with Extensions.cs in src/Modules/Notifications/
- [ ] T022 Register all 5 custom modules in API host in src/Couture.Api/Program.cs
- [ ] T023 Add PostgreSQL extensions (unaccent, pg_trgm) via initial migration in src/Couture.Api/Migrations.PostgreSQL/
- [ ] T024 Implement OrderStatus SmartEnum (8 statuses: RECUE, EN_ATTENTE, EN_COURS, BRODERIE, PERLAGE, RETOUCHE, PRETE, LIVREE with colors) in src/Modules/Orders/Orders/Domain/OrderStatus.cs
- [ ] T025 [P] Implement WorkType SmartEnum (SIMPLE, BRODE, PERLE, MIXTE with min delay + stall threshold) in src/Modules/Orders/Orders/Domain/WorkType.cs
- [ ] T026 [P] Implement PaymentMethod SmartEnum (ESPECES, VIREMENT, CCP, BARIDIMOB, DAHABIA) in src/Modules/Finance/Finance/Domain/PaymentMethod.cs
- [ ] T027 [P] Implement NotificationType SmartEnum (N01-N08 with priority + default channel) in src/Modules/Notifications/Notifications/Domain/NotificationType.cs
- [ ] T028 [P] Implement Role mapping: define workshop permissions (Orders.Create, Orders.View, etc.) in FSH Identity permissions in src/Modules/Identity/Identity/
- [ ] T029 Seed workshop roles (Manager, Tailor, Embroiderer, Beader, Cashier) with permission assignments via FSH Identity seeder
- [ ] T030 Implement BusinessDayCalculator service (Algerian weekend Fri/Sat + Holiday table) in src/Modules/Dashboard/Dashboard/Services/BusinessDayCalculator.cs
- [ ] T031 [P] Create Holiday entity + EF configuration + seed Algerian national holidays in src/Modules/Dashboard/Dashboard/Persistence/
- [ ] T032 [P] Create WorkshopSettings entity + EF configuration + seed defaults (max 10 orders/tailor, 8h session, SMS 08:00-20:00) in src/Modules/Orders/Orders/Persistence/
- [ ] T033 Create root layout with sidebar navigation, top navbar, and notification bell placeholder in frontend/src/routes/_layout.tsx
- [ ] T034 [P] Create auth layout guard (redirect to login if unauthenticated) in frontend/src/routes/_auth.tsx
- [ ] T035 [P] Create login page with username/password form in frontend/src/routes/auth/login.tsx
- [ ] T036 [P] Implement useAuth hook (login, logout, current user, role check) in frontend/src/hooks/use-auth.ts
- [ ] T037 [P] Create RoleGuard component for conditional rendering by role in frontend/src/components/layout/RoleGuard.tsx
- [ ] T038 [P] Create Flutter app shell with GoRouter, Material 3 theme, bottom navigation in mobile/lib/app/router.dart and mobile/lib/app/theme.dart
- [ ] T039 [P] Implement Flutter auth: login screen + JWT token storage in mobile/lib/core/auth/
- [ ] T040 Run initial EF Core migration for all module DbContexts in src/Couture.Api/Migrations.PostgreSQL/
- [ ] T041 Seed default admin user (admin@couture.local / Admin123!, Manager role) via FSH tenant provisioning

**Checkpoint**: All modules scaffolded, permissions seeded, auth working end-to-end (login on web + mobile), DB migrated

---

## Phase 3: User Story 1 — Create and Track a Couture Order (Priority: P1) MVP

**Goal**: Manager/tailor can create orders via 3-step form, system generates CMD-YYYY-NNNN code, calculates outstanding balance, and displays order list with filters

**Independent Test**: Create an order with a client, verify generated order number, check status is "Received", confirm balance = total - deposit

### Backend — Orders Module

- [ ] T042 [US1] Implement Order aggregate root (fields: Code, ClientId, Status, WorkType, Description, Fabric, TechnicalNotes, ReceptionDate, ExpectedDeliveryDate, TotalPrice, artisan IDs) with domain logic (CreateOrder factory, balance calculation, delivery date validation) in src/Modules/Orders/Orders/Domain/Order.cs
- [ ] T043 [P] [US1] Implement StatusTransition value object (FromStatus, ToStatus, Reason, TransitionedBy, TransitionedAt) in src/Modules/Orders/Orders/Domain/StatusTransition.cs
- [ ] T044 [P] [US1] Implement OrderPhoto entity in src/Modules/Orders/Orders/Domain/OrderPhoto.cs
- [ ] T045 [P] [US1] Implement strongly-typed IDs (OrderId, OrderPhotoId) via Vogen or FSH conventions in src/Modules/Orders/Orders.Contracts/
- [ ] T046 [US1] Create Order EF configuration (table mapping, indexes: Code unique, Status, ClientId, ExpectedDeliveryDate, artisan IDs) in src/Modules/Orders/Orders/Persistence/OrderConfiguration.cs
- [ ] T047 [P] [US1] Create StatusTransition EF configuration in src/Modules/Orders/Orders/Persistence/StatusTransitionConfiguration.cs
- [ ] T048 [P] [US1] Create OrderPhoto EF configuration in src/Modules/Orders/Orders/Persistence/OrderPhotoConfiguration.cs
- [ ] T049 [US1] Create OrdersDbContext registering Order, StatusTransition, OrderPhoto in src/Modules/Orders/Orders/Persistence/OrdersDbContext.cs
- [ ] T050 [US1] Implement sequential code generator (CMD-YYYY-NNNN, reset annually) as database sequence in src/Modules/Orders/Orders/Persistence/OrderCodeGenerator.cs
- [ ] T051 [US1] Implement CreateOrder command + handler + validator (3-step: client, work details, planning) with min delivery date enforcement in src/Modules/Orders/Orders/Features/CreateOrder/
- [ ] T052 [P] [US1] Define CreateOrder DTOs (request + response) in src/Modules/Orders/Orders.Contracts/Commands/CreateOrderCommand.cs
- [ ] T053 [US1] Implement CreateOrder minimal API endpoint (POST /api/v1/orders, auth: Manager+Tailor) in src/Modules/Orders/Orders/Features/CreateOrder/CreateOrderEndpoint.cs
- [ ] T054 [US1] Implement GetOrder query + handler (full detail with computed OutstandingBalance, IsLate, DelayDays) in src/Modules/Orders/Orders/Features/GetOrder/
- [ ] T055 [P] [US1] Implement ListOrders query + handler (paginated, filtered by status/workType/artisan/dateRange/search, 20 per page) in src/Modules/Orders/Orders/Features/ListOrders/
- [ ] T056 [US1] Implement ListOrders minimal API endpoint (GET /api/v1/orders with query params) in src/Modules/Orders/Orders/Features/ListOrders/ListOrdersEndpoint.cs
- [ ] T057 [US1] Implement GetOrder minimal API endpoint (GET /api/v1/orders/{id}) in src/Modules/Orders/Orders/Features/GetOrder/GetOrderEndpoint.cs
- [ ] T058 [US1] Implement UpdateOrder command + handler + validator (modify non-delivered orders: date, price, notes, artisan, photos) in src/Modules/Orders/Orders/Features/UpdateOrder/
- [ ] T059 [US1] Implement UpdateOrder minimal API endpoint (PUT /api/v1/orders/{id}) in src/Modules/Orders/Orders/Features/UpdateOrder/UpdateOrderEndpoint.cs
- [ ] T060 [US1] Implement UploadPhotos command + handler (multipart, max 10 images, local file storage) in src/Modules/Orders/Orders/Features/UploadPhotos/
- [ ] T061 [US1] Publish OrderCreated domain event on order creation in src/Modules/Orders/Orders.Contracts/Events/OrderCreatedEvent.cs
- [ ] T062 [US1] Run EF migration for Orders module in src/Couture.Api/Migrations.PostgreSQL/

### Backend — Clients Module (minimal for US1 — inline client creation)

- [ ] T063 [US1] Implement Client aggregate root (Code C-NNNN, FirstName, LastName, PrimaryPhone, SecondaryPhone, Address, Notes) in src/Modules/Clients/Clients/Domain/Client.cs
- [ ] T064 [P] [US1] Implement strongly-typed ClientId in src/Modules/Clients/Clients.Contracts/
- [ ] T065 [US1] Create Client EF configuration (indexes: Code unique, PrimaryPhone, trigram GIN on FirstName+LastName) in src/Modules/Clients/Clients/Persistence/ClientConfiguration.cs
- [ ] T066 [US1] Create ClientsDbContext in src/Modules/Clients/Clients/Persistence/ClientsDbContext.cs
- [ ] T067 [US1] Implement sequential client code generator (C-NNNN, never reset) in src/Modules/Clients/Clients/Persistence/ClientCodeGenerator.cs
- [ ] T068 [US1] Implement CreateClient command + handler + validator (phone format, duplicate detection) in src/Modules/Clients/Clients/Features/CreateClient/
- [ ] T069 [US1] Implement CreateClient minimal API endpoint (POST /api/v1/clients) in src/Modules/Clients/Clients/Features/CreateClient/CreateClientEndpoint.cs
- [ ] T070 [US1] Implement SearchClients query (partial name diacritic-insensitive + phone, debounced, max 10 results) in src/Modules/Clients/Clients/Features/SearchClients/
- [ ] T071 [US1] Implement SearchClients minimal API endpoint (GET /api/v1/clients/search?q=) in src/Modules/Clients/Clients/Features/SearchClients/SearchClientsEndpoint.cs
- [ ] T072 [US1] Run EF migration for Clients module in src/Couture.Api/Migrations.PostgreSQL/

### Frontend — Order Creation + List

- [ ] T073 [US1] Create StatusBadge component (colored badge per OrderStatus) in frontend/src/components/orders/StatusBadge.tsx
- [ ] T074 [P] [US1] Create WorkTypeBadge component in frontend/src/components/orders/WorkTypeBadge.tsx
- [ ] T075 [US1] Create useOrders hook (createOrder, getOrder, listOrders, updateOrder mutations/queries) in frontend/src/hooks/use-orders.ts
- [ ] T076 [P] [US1] Create useClients hook (createClient, searchClients) in frontend/src/hooks/use-clients.ts
- [ ] T077 [US1] Create ClientSearch combobox component (shadcn Command with debounced search, shows C-NNNN + name + phone) in frontend/src/components/clients/ClientSearch.tsx
- [ ] T078 [US1] Create 3-step order creation wizard: Step 1 (client select/create), Step 2 (work type + details), Step 3 (planning: date, price, deposit, artisan) in frontend/src/routes/orders/new.tsx
- [ ] T079 [US1] Create order list page with DataTable (shadcn), filters (status, workType, artisan, date range), search bar, pagination (20/page) in frontend/src/routes/orders/index.tsx
- [ ] T080 [US1] Create order detail page showing all fields, computed balance, photos, and edit button (if not LIVREE) in frontend/src/routes/orders/$orderId.tsx

### Mobile — Order List + Detail

- [ ] T081 [US1] Create order list screen with pull-to-refresh, status filter chips in mobile/lib/features/orders/order_list_screen.dart
- [ ] T082 [P] [US1] Create order detail screen showing key fields, balance, photos in mobile/lib/features/orders/order_detail_screen.dart
- [ ] T083 [US1] Implement orders Riverpod provider (list, detail, create) in mobile/lib/core/providers/orders_provider.dart

**Checkpoint**: Full order creation (3-step wizard) + list + detail working on web and mobile. Client search + inline creation working. CMD-YYYY-NNNN codes generating. Balance calculated. This is the MVP.

---

## Phase 4: User Story 2 — Manage Order Status Lifecycle (Priority: P1)

**Goal**: Artisans advance orders through status chain via action buttons. System enforces valid transitions, records timeline, calculates duration per status.

**Independent Test**: Move order Received -> In Progress -> Embroidery -> Ready -> Delivered. Verify timeline entries, invalid transitions rejected, delivery requires zero balance or manager reason.

### Backend

- [ ] T084 [US2] Implement status transition rules matrix (allowed transitions per WorkType with conditions) as domain logic in src/Modules/Orders/Orders/Domain/Order.cs (method: ChangeStatus)
- [ ] T085 [US2] Implement ChangeStatus command + handler + validator (enforce: tailor assigned for EN_COURS, embroiderer for BRODERIE, reason for RETOUCHE, balance/reason for LIVREE, actual delivery date for LIVREE) in src/Modules/Orders/Orders/Features/ChangeStatus/
- [ ] T086 [US2] Implement ChangeStatus minimal API endpoint (POST /api/v1/orders/{id}/status) in src/Modules/Orders/Orders/Features/ChangeStatus/ChangeStatusEndpoint.cs
- [ ] T087 [US2] Implement GetTimeline query + handler (all transitions with duration per status calculated) in src/Modules/Orders/Orders/Features/GetTimeline/
- [ ] T088 [US2] Implement GetTimeline minimal API endpoint (GET /api/v1/orders/{id}/timeline) in src/Modules/Orders/Orders/Features/GetTimeline/GetTimelineEndpoint.cs
- [ ] T089 [US2] Publish StatusChangedEvent domain event on each transition in src/Modules/Orders/Orders.Contracts/Events/StatusChangedEvent.cs
- [ ] T090 [US2] Add optimistic concurrency (RowVersion) on Order entity to prevent simultaneous status changes in src/Modules/Orders/Orders/Domain/Order.cs

### Frontend

- [ ] T091 [US2] Create Timeline component (vertical timeline with status icons, user, timestamp, duration per step) in frontend/src/components/orders/Timeline.tsx
- [ ] T092 [US2] Create StatusTransitionActions component (contextual buttons for valid next statuses, dialogs for reason/artisan assignment) in frontend/src/components/orders/StatusTransitionActions.tsx
- [ ] T093 [US2] Integrate Timeline + StatusTransitionActions into order detail page in frontend/src/routes/orders/$orderId.tsx
- [ ] T094 [US2] Add delivery dialog: require actual date + balance check (show warning if unpaid, require reason + manager confirm) in frontend/src/components/orders/DeliveryDialog.tsx

### Mobile

- [ ] T095 [US2] Add status action buttons to order detail screen (valid transitions only) in mobile/lib/features/orders/order_detail_screen.dart
- [ ] T096 [US2] Create timeline widget showing status history in mobile/lib/shared/widgets/timeline_widget.dart

**Checkpoint**: Full status lifecycle working. Timeline displays correctly. Invalid transitions blocked. Delivery with unpaid balance requires manager approval.

---

## Phase 5: User Story 3 — Quarterly Dashboard with KPIs (Priority: P1)

**Goal**: Manager sees 8 KPI cards, 5 analytics charts, quarter navigation, drill-down to filtered orders

**Independent Test**: Load dashboard with sample data, verify all 8 KPIs correct, click "late orders" badge filters order list, charts render accurately.

### Backend

- [ ] T097 [US3] Implement GetQuarterlyKPIs query + handler (aggregate: total, delivered, late, on-time rate, revenue, outstanding, embroidered count, beaded count, delta vs previous quarter) in src/Modules/Dashboard/Dashboard/Features/GetQuarterlyKPIs/
- [ ] T098 [US3] Implement GetQuarterlyKPIs minimal API endpoint (GET /api/v1/dashboard/kpis?year=&quarter=) in src/Modules/Dashboard/Dashboard/Features/GetQuarterlyKPIs/GetQuarterlyKPIsEndpoint.cs
- [ ] T099 [P] [US3] Implement GetMonthlyHistogram query (orders by month by work type for quarter) in src/Modules/Dashboard/Dashboard/Features/GetMonthlyHistogram/
- [ ] T100 [P] [US3] Implement GetStatusDistribution query (donut: count per status) in src/Modules/Dashboard/Dashboard/Features/GetStatusDistribution/
- [ ] T101 [P] [US3] Implement GetWorkTypeDistribution query (donut: count per work type) in src/Modules/Dashboard/Dashboard/Features/GetWorkTypeDistribution/
- [ ] T102 [P] [US3] Implement GetRevenueTrend query (line: revenue over last 4 quarters) in src/Modules/Dashboard/Dashboard/Features/GetRevenueTrend/
- [ ] T103 [P] [US3] Implement GetDelayByArtisan query (bar: avg delay top 5 artisans) in src/Modules/Dashboard/Dashboard/Features/GetDelayByArtisan/
- [ ] T104 [US3] Create minimal API endpoints for all 5 chart queries in src/Modules/Dashboard/Dashboard/Features/*/

### Frontend

- [ ] T105 [US3] Create KPICard component (value, label, delta %, color, clickable for drill-down) in frontend/src/components/dashboard/KPICard.tsx
- [ ] T106 [P] [US3] Create QuarterSelector component (prev/next arrows + dropdown) in frontend/src/components/dashboard/QuarterSelector.tsx
- [ ] T107 [US3] Create useDashboard hook (KPIs, all chart queries, parameterized by year+quarter) in frontend/src/hooks/use-dashboard.ts
- [ ] T108 [US3] Create dashboard page: 8 KPI cards grid + QuarterSelector + 5 Recharts (BarChart stacked, 2x PieChart, LineChart, BarChart) in frontend/src/routes/dashboard/index.tsx
- [ ] T109 [US3] Wire KPI card click to navigate to order list with pre-applied filters (e.g., lateOnly=true) in frontend/src/routes/dashboard/index.tsx

### Mobile

- [ ] T110 [US3] Create dashboard screen with 8 KPI cards (read-only, no charts) + quarter selector in mobile/lib/features/dashboard/dashboard_screen.dart

**Checkpoint**: Dashboard loads in <2s with all KPIs + charts. Quarter navigation works. Click-through to filtered order list works.

---

## Phase 6: User Story 4 — Proactive Notification System (Priority: P1)

**Goal**: Background jobs detect overdue/stalled orders, event-triggered alerts on status changes. In-app bell icon + SMS for critical alerts.

**Independent Test**: Simulate overdue order, verify N01 created in-app + SMS queued, notification appears in center with link to order.

### Backend

- [ ] T111 [US4] Implement Notification entity (Type, Priority, OrderId, RecipientId, Title, Message, Channel, IsRead, SmsStatus, ExpiresAt) in src/Modules/Notifications/Notifications/Domain/Notification.cs
- [ ] T112 [P] [US4] Implement NotificationConfig entity (per-type: IsEnabled, Channel, StallThresholds, SmsWindow) in src/Modules/Notifications/Notifications/Domain/NotificationConfig.cs
- [ ] T113 [US4] Create Notifications EF configurations + DbContext in src/Modules/Notifications/Notifications/Persistence/
- [ ] T114 [US4] Seed default NotificationConfig for N01-N08 in src/Modules/Notifications/Notifications/Persistence/
- [ ] T115 [US4] Implement ISmsGateway interface + MockSmsGateway (dev) in src/Modules/Notifications/Notifications/Sms/ISmsGateway.cs and MockSmsGateway.cs
- [ ] T116 [US4] Implement NotificationService (create notification, check SMS window, send SMS, log delivery status) in src/Modules/Notifications/Notifications/Domain/NotificationService.cs
- [ ] T117 [US4] Implement EvaluateOverdueOrders Hangfire recurring job (daily 02:00: N01 overdue, N02 24h, N03 48h) in src/Modules/Notifications/Notifications/Jobs/EvaluateOverdueOrdersJob.cs
- [ ] T118 [P] [US4] Implement EvaluateStalledOrders Hangfire recurring job (daily 02:00: N04 stalled per work type threshold) in src/Modules/Notifications/Notifications/Jobs/EvaluateStalledOrdersJob.cs
- [ ] T119 [US4] Subscribe to StatusChangedEvent: create N05 (retouche), N06 (ready), N07 (artisan assigned) notifications in src/Modules/Notifications/Notifications/EventHandlers/StatusChangedHandler.cs
- [ ] T120 [P] [US4] Subscribe to OrderDeliveredWithUnpaidEvent: create N08 notification in src/Modules/Notifications/Notifications/EventHandlers/
- [ ] T121 [US4] Implement SignalR NotificationHub (push new notifications + unread count to connected clients) in src/Modules/Notifications/Notifications/Hub/NotificationHub.cs
- [ ] T122 [US4] Implement ListNotifications query + endpoint (GET /api/v1/notifications?filter=all|unread|critical, paginated) in src/Modules/Notifications/Notifications/Features/ListNotifications/
- [ ] T123 [P] [US4] Implement GetUnreadCount query + endpoint (GET /api/v1/notifications/unread-count) in src/Modules/Notifications/Notifications/Features/GetUnreadCount/
- [ ] T124 [US4] Implement MarkRead + MarkAllRead commands + endpoints in src/Modules/Notifications/Notifications/Features/MarkRead/
- [ ] T125 [US4] Implement ConfigureAlerts command + endpoint (PUT /api/v1/notifications/config/{type}) in src/Modules/Notifications/Notifications/Features/ConfigureAlerts/
- [ ] T126 [P] [US4] Implement GetSmsLogs query + endpoint (GET /api/v1/notifications/sms-logs) in src/Modules/Notifications/Notifications/Features/GetSmsLogs/
- [ ] T127 [US4] Implement PurgeExpiredNotifications Hangfire job (delete > 30 days) in src/Modules/Notifications/Notifications/Jobs/PurgeExpiredNotificationsJob.cs
- [ ] T128 [US4] Register all Hangfire recurring jobs in module Extensions.cs in src/Modules/Notifications/Notifications/Extensions.cs
- [ ] T129 [US4] Run EF migration for Notifications module in src/Couture.Api/Migrations.PostgreSQL/

### Frontend

- [ ] T130 [US4] Create SignalR client service (connect on auth, listen for ReceiveNotification + UnreadCountChanged) in frontend/src/services/signalr-client.ts
- [ ] T131 [US4] Create NotificationBell component (bell icon + unread count badge, click opens panel) in frontend/src/components/notifications/NotificationBell.tsx
- [ ] T132 [US4] Create NotificationPanel component (sheet/slide-over with tabs: All/Unread/Critical, mark-as-read, click-through to order) in frontend/src/components/notifications/NotificationPanel.tsx
- [ ] T133 [US4] Integrate NotificationBell into root layout navbar in frontend/src/routes/_layout.tsx
- [ ] T134 [US4] Create full notification center page (paginated list, filters) in frontend/src/routes/notifications/index.tsx
- [ ] T135 [P] [US4] Create notification settings admin page (enable/disable per type, thresholds, SMS window) in frontend/src/routes/admin/notifications.tsx
- [ ] T136 [US4] Create useNotifications hook (list, unreadCount, markRead, config) in frontend/src/hooks/use-notifications.ts

### Mobile

- [ ] T137 [US4] Create notification screen with list (tabs: All/Unread/Critical) + mark-as-read in mobile/lib/features/notifications/notification_screen.dart
- [ ] T138 [US4] Add notification badge to bottom navigation bar in mobile/lib/app/router.dart

**Checkpoint**: Notifications working end-to-end. Background jobs evaluate overdue/stalled. Status change events create alerts. Bell icon shows unread count. SMS sent (mock in dev). Manager can configure thresholds.

---

## Phase 7: User Story 5 — Client Management with Measurements (Priority: P2)

**Goal**: Full client registry with customizable measurement grid, historical tracking, search, order history, duplicate detection

**Independent Test**: Create client with measurements, modify measurements, verify history shows dated entries. Search by partial name returns results within 300ms.

### Backend

- [ ] T139 [US5] Implement MeasurementField entity (Name, Unit, DisplayOrder, IsDefault, IsActive) in src/Modules/Clients/Clients/Domain/MeasurementField.cs
- [ ] T140 [P] [US5] Implement ClientMeasurement entity (ClientId, MeasurementFieldId, Value, MeasuredAt, MeasuredBy) in src/Modules/Clients/Clients/Domain/ClientMeasurement.cs
- [ ] T141 [US5] Create MeasurementField + ClientMeasurement EF configurations in src/Modules/Clients/Clients/Persistence/
- [ ] T142 [US5] Seed 10 default measurement fields (Tour de poitrine, Tour de taille, etc.) in src/Modules/Clients/Clients/Persistence/
- [ ] T143 [US5] Implement GetClient query + handler (full detail with current measurements, stats: totalOrders, totalCollected, lastVisit, activeOrders) in src/Modules/Clients/Clients/Features/GetClient/
- [ ] T144 [US5] Implement GetClient minimal API endpoint (GET /api/v1/clients/{id}) in src/Modules/Clients/Clients/Features/GetClient/GetClientEndpoint.cs
- [ ] T145 [US5] Implement UpdateClient command + endpoint (PUT /api/v1/clients/{id}) in src/Modules/Clients/Clients/Features/UpdateClient/
- [ ] T146 [US5] Implement RecordMeasurements command + handler (append new measurements, preserve history) in src/Modules/Clients/Clients/Features/RecordMeasurements/
- [ ] T147 [US5] Implement RecordMeasurements endpoint (POST /api/v1/clients/{id}/measurements) in src/Modules/Clients/Clients/Features/RecordMeasurements/
- [ ] T148 [US5] Implement GetMeasurementHistory query + endpoint (GET /api/v1/clients/{id}/measurements) in src/Modules/Clients/Clients/Features/GetMeasurementHistory/
- [ ] T149 [US5] Implement GetClientOrders query + endpoint (GET /api/v1/clients/{id}/orders, paginated) in src/Modules/Clients/Clients/Features/GetClientOrders/
- [ ] T150 [US5] Implement ListClients query + endpoint (GET /api/v1/clients, paginated) in src/Modules/Clients/Clients/Features/ListClients/
- [ ] T151 [US5] Implement ManageMeasurementFields CRUD endpoints (GET/POST/PUT/DELETE /api/v1/measurement-fields, Manager only) in src/Modules/Clients/Clients/Features/ManageMeasurementFields/
- [ ] T152 [US5] Run EF migration for Clients measurement entities in src/Couture.Api/Migrations.PostgreSQL/

### Frontend

- [ ] T153 [US5] Create MeasurementGrid component (editable grid of current measurements, add custom field) in frontend/src/components/clients/MeasurementGrid.tsx
- [ ] T154 [P] [US5] Create ClientStats component (total orders, total collected, last visit, active orders) in frontend/src/components/clients/ClientStats.tsx
- [ ] T155 [US5] Create client list page with DataTable + search bar in frontend/src/routes/clients/index.tsx
- [ ] T156 [US5] Create client detail page (info, measurements with history, order history, stats) in frontend/src/routes/clients/$clientId.tsx
- [ ] T157 [US5] Update useClients hook with full CRUD + measurements in frontend/src/hooks/use-clients.ts

### Mobile

- [ ] T158 [US5] Create client list screen with search in mobile/lib/features/clients/client_list_screen.dart
- [ ] T159 [US5] Create client detail screen with measurements in mobile/lib/features/clients/client_detail_screen.dart

**Checkpoint**: Full client CRUD with measurements, history tracking, diacritic-insensitive search, duplicate detection, order history view.

---

## Phase 8: User Story 6 — User Accounts and Role-Based Access (Priority: P2)

**Goal**: Manager creates staff accounts with roles, each role sees only authorized data. Optional SMS 2FA.

**Independent Test**: Create users with different roles, verify tailor sees only assigned orders, cashier sees only finance, embroiderer sees only embroidery-phase orders.

### Backend

- [ ] T160 [US6] Extend FSH Identity: add workshop-specific user fields (phone for SMS, multi-role support, session duration preference) in src/Modules/Identity/Identity/
- [ ] T161 [US6] Implement SMS OTP 2FA flow: generate OTP, send via ISmsGateway, verify OTP endpoint in src/Modules/Identity/Identity/Features/
- [ ] T162 [US6] Implement role-based order filtering: Tailor sees only AssignedTailorId = self, Embroiderer sees only orders in BRODERIE status assigned to self, Beader sees only PERLAGE status assigned to self in src/Modules/Orders/Orders/Features/ListOrders/
- [ ] T163 [US6] Implement artisan list endpoint (GET /api/v1/users/artisans?role=&availableOnly=) with active order count + capacity check in src/Modules/Identity/Identity/Features/
- [ ] T164 [US6] Implement configurable session duration (8h/24h/7d) with auto-logout on inactivity in JWT token expiration config

### Frontend

- [ ] T165 [US6] Create user management admin page (CRUD users, assign roles, activate/deactivate, reset password) in frontend/src/routes/admin/users.tsx
- [ ] T166 [US6] Add OTP verification step to login flow (show OTP input after credentials if 2FA enabled) in frontend/src/routes/auth/login.tsx
- [ ] T167 [US6] Implement role-based sidebar navigation (hide/show menu items per role) in frontend/src/routes/_layout.tsx

### Mobile

- [ ] T168 [US6] Add OTP screen to mobile login flow in mobile/lib/core/auth/
- [ ] T169 [US6] Apply role-based filtering to mobile order list (artisan sees only their orders) in mobile/lib/features/orders/order_list_screen.dart

**Checkpoint**: Multi-user working with role isolation. Tailor sees own orders only. Cashier sees finance only. 2FA via SMS OTP working.

---

## Phase 9: User Story 7 — Financial Tracking and Receipts (Priority: P2)

**Goal**: Record payments, track balances, generate PDF receipts, financial dashboard view

**Independent Test**: Record multiple partial payments, verify balance updates, PDF receipt generated with correct details.

### Backend

- [ ] T170 [US7] Implement Payment entity (Amount, PaymentMethod, PaymentDate, Note, RecordedBy) in src/Modules/Finance/Finance/Domain/Payment.cs
- [ ] T171 [P] [US7] Implement Receipt entity (Code REC-YYYY-NNNN, PaymentId, PdfStoragePath) in src/Modules/Finance/Finance/Domain/Receipt.cs
- [ ] T172 [US7] Create Payment + Receipt EF configurations + FinanceDbContext in src/Modules/Finance/Finance/Persistence/
- [ ] T173 [US7] Implement receipt code generator (REC-YYYY-NNNN, reset annually) in src/Modules/Finance/Finance/Persistence/ReceiptCodeGenerator.cs
- [ ] T174 [US7] Implement RecordPayment command + handler (validate amount <= outstanding, create payment + receipt, update order HasUnpaidBalance flag) in src/Modules/Finance/Finance/Features/RecordPayment/
- [ ] T175 [US7] Implement RecordPayment endpoint (POST /api/v1/orders/{orderId}/payments) in src/Modules/Finance/Finance/Features/RecordPayment/
- [ ] T176 [US7] Implement QuestPDF receipt template (workshop header + logo, REC code, date, order+client info, amount, method, cumulative, remaining) in src/Modules/Finance/Finance/Pdf/ReceiptPdfGenerator.cs
- [ ] T177 [US7] Implement DownloadReceipt endpoint (GET /api/v1/finance/receipts/{id}/pdf) in src/Modules/Finance/Finance/Features/DownloadReceipt/
- [ ] T178 [US7] Implement GetPayments query + endpoint (GET /api/v1/orders/{orderId}/payments) in src/Modules/Finance/Finance/Features/GetPayments/
- [ ] T179 [US7] Implement GetFinancialSummary query + endpoint (GET /api/v1/finance/summary?year=&quarter= — revenue by method, outstanding, unpaid delivered) in src/Modules/Finance/Finance/Features/GetFinancialSummary/
- [ ] T180 [US7] Publish PaymentRecordedEvent domain event in src/Modules/Finance/Finance.Contracts/Events/PaymentRecordedEvent.cs
- [ ] T181 [US7] Run EF migration for Finance module in src/Couture.Api/Migrations.PostgreSQL/

### Frontend

- [ ] T182 [US7] Create PaymentForm component (amount, method select, date, note) in frontend/src/components/finance/PaymentForm.tsx
- [ ] T183 [P] [US7] Create PaymentHistory component (table of payments with receipt download links) in frontend/src/components/finance/PaymentHistory.tsx
- [ ] T184 [US7] Integrate PaymentForm + PaymentHistory into order detail page in frontend/src/routes/orders/$orderId.tsx
- [ ] T185 [US7] Create financial dashboard page (revenue by method chart, outstanding list, unpaid delivered alerts) in frontend/src/routes/finance/index.tsx
- [ ] T186 [US7] Create useFinance hook (recordPayment, getPayments, getFinancialSummary, downloadReceipt) in frontend/src/hooks/use-finance.ts

### Mobile

- [ ] T187 [US7] Create payment recording screen in mobile/lib/features/finance/payment_screen.dart
- [ ] T188 [US7] Add payment summary to order detail on mobile in mobile/lib/features/orders/order_detail_screen.dart

**Checkpoint**: Full payment lifecycle. PDF receipts generating. Financial dashboard showing revenue breakdown. Outstanding balances tracked accurately.

---

## Phase 10: User Story 8 — Multi-Artisan Assignment for Mixed Orders (Priority: P2)

**Goal**: Mixed orders assign up to 3 artisans. Each artisan sees orders only in their active phase. Phase transitions trigger notifications.

**Independent Test**: Create mixed order, assign 3 artisans, verify each sees order only during their phase, notification sent at phase transition.

### Backend

- [ ] T189 [US8] Extend Order aggregate: validate multi-artisan assignment rules (embroiderer required for BRODE/MIXTE, beader for PERLE/MIXTE) in src/Modules/Orders/Orders/Domain/Order.cs
- [ ] T190 [US8] Update ListOrders filtering: Embroiderer sees only orders where AssignedEmbroidererId = self AND Status = BRODERIE; Beader sees only AssignedBeaderId = self AND Status = PERLAGE in src/Modules/Orders/Orders/Features/ListOrders/
- [ ] T191 [US8] Update ChangeStatus handler: on BRODERIE -> PERLAGE transition, trigger N07 to beader in src/Modules/Orders/Orders/Features/ChangeStatus/
- [ ] T192 [US8] Update CreateOrder handler: validate and store AssignedEmbroidererId + AssignedBeaderId for BRODE/PERLE/MIXTE types in src/Modules/Orders/Orders/Features/CreateOrder/

### Frontend

- [ ] T193 [US8] Update order creation wizard Step 3: show embroiderer/beader assignment fields conditionally by WorkType in frontend/src/routes/orders/new.tsx
- [ ] T194 [US8] Update order detail: show all assigned artisans with their phases in frontend/src/routes/orders/$orderId.tsx

### Mobile

- [ ] T195 [US8] Update order detail: display assigned artisans per phase in mobile/lib/features/orders/order_detail_screen.dart

**Checkpoint**: Multi-artisan assignment working. Phase-based visibility enforced. Artisan-specific notifications triggered.

---

## Phase 11: User Story 9 — Offline Mode with Deferred Sync (Priority: P3)

**Goal**: Mobile app works fully offline with local storage. Syncs on reconnect within 5 seconds. Conflicts detected.

**Independent Test**: Go offline, create order + change status, reconnect, verify sync within 5s with no data loss.

### Backend

- [ ] T196 [US9] Implement sync endpoint (POST /api/v1/sync) accepting batched offline operations with timestamps + row versions in src/Modules/Orders/Orders/Features/Sync/
- [ ] T197 [US9] Implement conflict detection (optimistic concurrency: reject if server version > client version, return conflicts) in src/Modules/Orders/Orders/Features/Sync/

### Mobile

- [ ] T198 [US9] Implement Hive-based local storage for orders, clients, payments in mobile/lib/core/storage/
- [ ] T199 [US9] Implement offline operation queue (store pending creates, status changes, payments with timestamps) in mobile/lib/core/sync/offline_queue.dart
- [ ] T200 [US9] Implement sync service (on connectivity change: send queued ops, handle conflicts, update local store) in mobile/lib/core/sync/sync_service.dart
- [ ] T201 [US9] Implement connectivity monitor (connectivity_plus) + sync trigger in mobile/lib/core/sync/connectivity_monitor.dart
- [ ] T202 [US9] Create conflict resolution screen (show conflicting records, manager resolves) in mobile/lib/core/sync/conflict_screen.dart
- [ ] T203 [US9] Add offline indicator banner to app shell in mobile/lib/app/router.dart

**Checkpoint**: Full offline operation on mobile. Sync within 5s on reconnect. Conflicts shown for manager resolution.

---

## Phase 12: User Story 10 — Reports and Data Export (Priority: P3)

**Goal**: Export quarterly data in CSV, Excel (.xlsx with formatting), PDF (complete report)

**Independent Test**: Export quarter data in each format, verify file contents match dashboard data.

### Backend

- [ ] T204 [US10] Implement CSV export service (quarterly orders as CSV) in src/Modules/Dashboard/Dashboard/Services/CsvExportService.cs
- [ ] T205 [P] [US10] Implement Excel export service (ClosedXML: .xlsx with headers, color-coded statuses) in src/Modules/Dashboard/Dashboard/Services/ExcelExportService.cs
- [ ] T206 [P] [US10] Implement PDF report service (QuestPDF: KPI summary + charts as images + order table) in src/Modules/Dashboard/Dashboard/Services/PdfReportService.cs
- [ ] T207 [US10] Implement ExportReport endpoint (GET /api/v1/dashboard/export?year=&quarter=&format=csv|xlsx|pdf) in src/Modules/Dashboard/Dashboard/Features/ExportReport/

### Frontend

- [ ] T208 [US10] Add export buttons (CSV/Excel/PDF) to dashboard page with format selector in frontend/src/routes/dashboard/index.tsx
- [ ] T209 [US10] Implement file download handler (fetch blob, trigger browser download) in frontend/src/services/download.ts

**Checkpoint**: All 3 export formats working from dashboard. Files contain correct formatted data.

---

## Phase 13: Polish & Cross-Cutting Concerns

**Purpose**: Improvements affecting multiple stories

- [ ] T210 [P] Implement workshop settings admin page (name, address, phone, logo upload, max orders/tailor, default session) in frontend/src/routes/admin/settings.tsx
- [ ] T211 [P] Implement holiday management on settings page (CRUD holidays for business day calc) in frontend/src/routes/admin/settings.tsx
- [ ] T212 [P] Add artisan capacity warning on order creation (non-blocking toast when tailor at max active orders) across frontend/src/routes/orders/new.tsx
- [ ] T213 Add French (fr-DZ) localization strings for all UI labels, error messages, status names in frontend/src/lib/i18n.ts
- [ ] T214 [P] Add French localization to Flutter app in mobile/lib/core/utils/l10n/
- [ ] T215 Configure Docker Compose for production deployment (API + Postgres + Redis + Nginx) in docker-compose.yml
- [ ] T216 [P] Configure Flutter CI builds (APK for Android, IPA for iOS) in .github/workflows/ or equivalent
- [ ] T217 Add Playwright E2E smoke test: login -> create order -> change status -> verify timeline in frontend/tests/e2e/order-lifecycle.spec.ts
- [ ] T218 Run quickstart.md validation: verify all steps work end-to-end

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — BLOCKS all user stories
- **Phase 3-6 (P1 stories)**: All depend on Phase 2 completion
  - US1 (Orders) → must complete before US2 (Status Lifecycle), US3 (Dashboard), US4 (Notifications)
  - US2, US3, US4 can proceed in parallel after US1
- **Phase 7-10 (P2 stories)**: Depend on Phase 2, enhanced by P1 stories
  - US5 (Clients) → independent, can start after Phase 2
  - US6 (RBAC) → independent, can start after Phase 2
  - US7 (Finance) → depends on US1 (needs Order entity for payments)
  - US8 (Multi-Artisan) → depends on US1 + US2 (extends order creation + status transitions)
- **Phase 11-12 (P3 stories)**: Depend on P1 and P2 stories
  - US9 (Offline) → depends on US1, US2, US7 (needs all core flows)
  - US10 (Export) → depends on US3 (extends dashboard)
- **Phase 13 (Polish)**: After all desired stories complete

### User Story Dependency Graph

```text
Phase 2 (Foundational)
    │
    ├── US1 (Orders) ────────┬── US2 (Status) ──┐
    │                        ├── US3 (Dashboard) ├── US10 (Export)
    │                        ├── US4 (Notifications)
    │                        ├── US7 (Finance)
    │                        └── US8 (Multi-Artisan, needs US2)
    ├── US5 (Clients) ──────── independent
    └── US6 (RBAC) ─────────── independent
                                    │
                              US9 (Offline, needs US1+US2+US7)
```

### Within Each User Story

1. Domain entities → EF configurations → Migrations
2. Commands/Queries → Handlers → Validators → Endpoints
3. Frontend hooks → Components → Pages
4. Mobile providers → Screens

### Parallel Opportunities

**After Phase 2 completes:**
- Team of 3: Dev A → US1, Dev B → US5, Dev C → US6
- After US1: Dev A → US2, Dev B → US3 (parallel), Dev C → US4 (parallel)
- After US2: US7 + US8 can proceed in parallel

**Within each story (marked [P]):**
- SmartEnums and DTOs can be written in parallel
- EF configurations for independent entities can be written in parallel
- Frontend components for different parts of the page can be written in parallel
- Chart queries (US3) can all be implemented in parallel

---

## Parallel Example: User Story 1

```bash
# Launch parallel domain entities:
Task T042: "Order aggregate in Orders/Domain/Order.cs"
Task T043: "StatusTransition VO in Orders/Domain/StatusTransition.cs"
Task T044: "OrderPhoto entity in Orders/Domain/OrderPhoto.cs"
Task T045: "Strongly-typed IDs in Orders.Contracts/"

# Launch parallel EF configs (after entities):
Task T046: "Order EF config"
Task T047: "StatusTransition EF config"
Task T048: "OrderPhoto EF config"

# Launch parallel frontend components:
Task T073: "StatusBadge component"
Task T074: "WorkTypeBadge component"
Task T075: "useOrders hook"
Task T076: "useClients hook"
```

---

## Parallel Example: User Story 3 (Dashboard Charts)

```bash
# All 5 chart queries can be implemented in parallel:
Task T099: "GetMonthlyHistogram"
Task T100: "GetStatusDistribution"
Task T101: "GetWorkTypeDistribution"
Task T102: "GetRevenueTrend"
Task T103: "GetDelayByArtisan"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (FSH + React + Flutter bootstrap)
2. Complete Phase 2: Foundational (modules, permissions, auth, DB)
3. Complete Phase 3: US1 — Create & Track Orders
4. **STOP and VALIDATE**: Create an order, verify code, balance, list, search
5. Deploy/demo — this is immediately useful, replacing paper tracking

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. US1 (Orders) → MVP! Workshop can start tracking orders
3. US2 (Status Lifecycle) → Full workflow tracking with timeline
4. US3 (Dashboard) + US4 (Notifications) → Manager gets visibility + proactive alerts
5. US5 (Clients) + US6 (RBAC) + US7 (Finance) → Multi-user + financial tracking
6. US8 (Multi-Artisan) → Specialized workshop workflows
7. US9 (Offline) + US10 (Export) → Resilience + reporting
8. Polish → Production readiness

### Parallel Team Strategy (3 developers)

1. All three: Phase 1 + Phase 2 together
2. After Foundational:
   - **Dev A (Backend focus)**: US1 backend → US2 backend → US7 backend → US8
   - **Dev B (Frontend focus)**: US1 frontend → US3 → US5 frontend → US10
   - **Dev C (Mobile + Infra)**: US1 mobile → US4 → US6 → US9
3. Phase 13 (Polish): All three in parallel

---

## Summary

| Metric | Value |
|--------|-------|
| **Total tasks** | 218 |
| **Phase 1 (Setup)** | 16 tasks |
| **Phase 2 (Foundational)** | 25 tasks |
| **US1 — Orders (P1 MVP)** | 42 tasks |
| **US2 — Status Lifecycle (P1)** | 13 tasks |
| **US3 — Dashboard (P1)** | 14 tasks |
| **US4 — Notifications (P1)** | 29 tasks |
| **US5 — Clients (P2)** | 21 tasks |
| **US6 — RBAC (P2)** | 10 tasks |
| **US7 — Finance (P2)** | 19 tasks |
| **US8 — Multi-Artisan (P2)** | 7 tasks |
| **US9 — Offline (P3)** | 8 tasks |
| **US10 — Export (P3)** | 6 tasks |
| **Polish** | 9 tasks |
| **Parallel opportunities** | 68 tasks marked [P] |
| **Suggested MVP** | US1 only (42 tasks after foundation) |

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable after its checkpoint
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- FSH built-in modules (Identity, Auditing, Multitenancy) are pre-configured in Phase 2, not reimplemented
