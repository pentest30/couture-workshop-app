# Feature Specification: Atelier Couture Workshop App

**Feature Branch**: `002-couture-workshop-app`
**Created**: 2026-03-24
**Status**: Draft
**Input**: Digitalize the operational management of an artisanal couture workshop — order management, work types (embroidered, beaded, mixed), quarterly dashboard, notifications, clients, user roles, and financial tracking.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create and Track a Couture Order (Priority: P1)

As a workshop manager or tailor, I need to create a new couture order by selecting or creating a client, specifying the work type (Simple/Embroidered/Beaded/Mixed), setting delivery dates and pricing, and assigning artisans. The system generates a unique order number (CMD-YYYY-NNNN), sets the initial status to "Received", and calculates the outstanding balance. I can then track the order through its entire lifecycle from reception to delivery.

**Why this priority**: Order creation and tracking is the core value proposition of the application. Without it, no other module (dashboard, notifications, finance) can function. This is the minimum viable product.

**Independent Test**: Can be fully tested by creating an order with a client, verifying the generated order number, checking the status is "Received", and confirming the balance is calculated correctly. Delivers immediate value by replacing paper-based order tracking.

**Acceptance Scenarios**:

1. **Given** a registered client and an authenticated manager, **When** the manager fills the 3-step order form (client selection, work details, planning), **Then** a new order is created with a unique sequential number (CMD-YYYY-NNNN), status "Received", and the outstanding balance equals total price minus deposit.
2. **Given** a new walk-in client, **When** the manager creates a new client inline during order creation, **Then** the client is saved with an auto-generated code (C-NNNN) and the order form is pre-filled with that client.
3. **Given** an order of type "Embroidered", **When** the manager fills the order form, **Then** additional fields for embroidery style, thread colors, density, and embroidery zone are displayed and required.
4. **Given** a completed order form, **When** the manager submits it, **Then** a deposit receipt PDF is automatically generated and the assigned artisan receives an in-app and SMS notification.

---

### User Story 2 - Manage Order Status Lifecycle (Priority: P1)

As a tailor, embroiderer, or beader, I need to advance the order through its status lifecycle using contextual action buttons. The system enforces valid transitions (e.g., "In Progress" can go to "Embroidery" only for embroidered/mixed types), records each transition in a visual timeline with timestamps and actor identity, and triggers notifications to relevant stakeholders.

**Why this priority**: Status management is inseparable from order tracking — it drives the entire workshop workflow and is the basis for notifications, dashboard metrics, and delivery tracking.

**Independent Test**: Can be tested by moving an order through the full status chain (Received -> In Progress -> Embroidery -> Ready -> Delivered) and verifying each transition is recorded in the timeline with the correct user, timestamp, and that invalid transitions are rejected.

**Acceptance Scenarios**:

1. **Given** an order in status "In Progress" of type "Embroidered", **When** the tailor clicks "Move to Embroidery" and assigns an embroiderer, **Then** the status changes to "Embroidery", the timeline records the transition, and the embroiderer receives notification N07.
2. **Given** an order in any active status, **When** a user attempts to move it to "Alteration" (Retouche), **Then** the system requires a mandatory alteration reason before allowing the transition.
3. **Given** an order in status "Ready" with an outstanding balance of 0 DZD, **When** the manager marks it as "Delivered" and enters the actual delivery date, **Then** the status becomes "Delivered" (irreversible), and a final delivery receipt PDF is generated.
4. **Given** an order in status "Ready" with an unpaid balance, **When** the manager attempts to deliver, **Then** the system requires explicit manager validation with a mandatory reason, and notification N08 is triggered.
5. **Given** an order in status "Delivered", **When** any user attempts to modify it, **Then** the system prevents all modifications except adding a note by the manager.

---

### User Story 3 - Quarterly Dashboard with KPIs (Priority: P1)

As a workshop manager, I need a dashboard that loads by default upon login showing the current quarter's key performance indicators: total orders, delivered orders, late orders, on-time delivery rate, revenue collected, outstanding balances, and breakdowns by work type. I can navigate between quarters and drill down from KPI badges into filtered order lists.

**Why this priority**: The dashboard provides the manager with operational visibility and decision-making capability. It is the primary screen and aggregates data from all other modules.

**Independent Test**: Can be tested by loading the dashboard with sample data for a quarter and verifying all 8 KPI cards show correct values, clicking the "late orders" badge filters the list correctly, and charts render with accurate data.

**Acceptance Scenarios**:

1. **Given** an authenticated manager, **When** the dashboard loads, **Then** the current quarter is selected by default and all 8 KPI cards display correct aggregated values with delta percentage versus the previous quarter.
2. **Given** the dashboard is showing Q1 2026, **When** the manager clicks the "3 late orders" red badge, **Then** the order list filters to show only orders where expected delivery date has passed and status is not "Delivered".
3. **Given** quarterly data exists, **When** the manager views the analytics section, **Then** a monthly histogram (stacked by work type), status donut chart, work type donut chart, revenue trend curve (last 4 quarters), and average delay bar chart (top 5 tailors) are displayed.
4. **Given** the filtered order list, **When** the manager clicks "Export CSV/Excel/PDF", **Then** the system generates a downloadable file with the correct data and formatting.

---

### User Story 4 - Proactive Notification System (Priority: P1)

As any workshop user, I receive timely alerts about critical events: overdue deliveries, upcoming deadlines (24h/48h), stalled orders, status changes to alteration, new order assignments, and delivery attempts with unpaid balances. Notifications appear in-app via a bell icon with unread count badge and, for high-priority alerts, via SMS within configurable hours.

**Why this priority**: Notifications are the proactive engine that prevents delays from escalating. Without them, the manager discovers problems reactively, defeating the purpose of digitalization.

**Independent Test**: Can be tested by simulating an overdue order and verifying that notification N01 is created in-app for the tailor and manager, SMS is sent within the configured time window, and the notification appears in the notification center with a link to the order.

**Acceptance Scenarios**:

1. **Given** an order whose expected delivery date has passed by 1 day, **When** the daily background job runs, **Then** notification N01 (critical) is sent in-app and via SMS to the assigned artisan and the manager.
2. **Given** an order of type "Beaded" that has been in the same status for 12 days (threshold: 10 days), **When** the background job evaluates active orders, **Then** notification N04 is sent to the manager indicating the order is stalled.
3. **Given** the notification center, **When** a user opens it, **Then** they see notifications sorted by recency with priority icons, can filter by "All / Unread / Critical", and can click through to the related order.
4. **Given** the manager's notification settings screen, **When** the manager modifies thresholds for stalled order alerts per work type, **Then** future background job evaluations use the updated thresholds.

---

### User Story 5 - Client Management with Measurements (Priority: P2)

As a manager or tailor, I need to maintain a client registry with personal details, customizable body measurements, and order history. I can search for clients by number, name, or phone number with real-time results. Each client has a measurement grid that the manager can customize (add/remove fields), and all measurement changes are historically tracked.

**Why this priority**: Clients are essential for order creation but the system can initially function with minimal client data. Full measurement tracking and history add significant value for repeat customers but are not blocking for MVP launch.

**Independent Test**: Can be tested by creating a client with measurements, modifying measurements, and verifying the history shows dated entries. Search by partial name and phone number returns correct results within 300ms.

**Acceptance Scenarios**:

1. **Given** the client creation form, **When** a user enters name, surname, and phone (Algerian format: 05/06/07XXXXXXXX), **Then** the client is saved with an auto-generated code (C-NNNN).
2. **Given** an existing client with measurements, **When** the tailor updates a measurement value, **Then** the previous value is preserved in the measurement history with a timestamp.
3. **Given** the client search field, **When** a user types a partial name (case and diacritic insensitive), **Then** matching clients appear in real-time (debounced 300ms) showing client number, name, phone, and active order count.
4. **Given** the client file, **When** the user views order history, **Then** all orders for that client are listed with statistics: total orders, total amount collected, last visit date, and current active orders.
5. **Given** client creation with a phone number that already exists, **When** submitting, **Then** the system alerts about a potential duplicate and offers to select the existing client or confirm creation.

---

### User Story 6 - User Accounts and Role-Based Access (Priority: P2)

As a workshop manager, I need to create user accounts for my staff (tailors, embroiderers, beaders, cashiers) and assign them roles that control what they can see and do. Tailors see only their assigned orders, cashiers access only the financial module, and only the manager has full access. Authentication is via username/password with optional SMS-based two-factor authentication.

**Why this priority**: Role-based access is important for data security and workflow isolation but the system can initially operate in single-user mode (manager) before expanding to multi-user.

**Independent Test**: Can be tested by creating users with different roles, logging in as each, and verifying that a tailor sees only their orders, a cashier sees only finance, and an embroiderer sees only orders in the embroidery phase assigned to them.

**Acceptance Scenarios**:

1. **Given** the manager's admin panel, **When** the manager creates a new user with role "Tailor", **Then** that user can log in and sees only orders assigned to them.
2. **Given** a user with role "Cashier", **When** they log in, **Then** they can access payment collection and receipt generation but cannot create orders or change statuses.
3. **Given** a user with multiple roles (e.g., Tailor + Embroiderer), **When** they log in, **Then** they see orders assigned to them in both their tailoring and embroidery phases.
4. **Given** the login screen, **When** a user enters valid credentials and 2FA is enabled, **Then** they receive an SMS OTP and must enter it to complete login.
5. **Given** a configurable session duration, **When** a user is inactive beyond the configured period, **Then** they are automatically logged out.

---

### User Story 7 - Financial Tracking and Receipts (Priority: P2)

As a manager or cashier, I need to record payments against orders (deposits, partial payments, final settlement), track outstanding balances, and automatically generate PDF receipts for each payment. The financial dashboard view shows quarterly revenue by payment method, outstanding balances, and flags delivered orders with unpaid amounts.

**Why this priority**: Financial tracking completes the order lifecycle and is critical for business operations, but the core order workflow can function with basic deposit/balance tracking before full payment history is needed.

**Independent Test**: Can be tested by recording multiple partial payments on an order, verifying the balance updates correctly, and checking that a PDF receipt is generated for each payment with correct details.

**Acceptance Scenarios**:

1. **Given** an order with a total price and initial deposit, **When** the cashier records an additional payment specifying amount, method (Cash/Transfer/CCP/BaridiMob/Dahabia), and date, **Then** the outstanding balance is recalculated and a PDF receipt is generated.
2. **Given** the financial dashboard, **When** the manager views quarterly data, **Then** revenue is broken down by payment method, outstanding balances are listed, and delivered orders with unpaid amounts are flagged with alerts.
3. **Given** a generated receipt PDF, **When** it is opened, **Then** it displays the workshop header (name, address, logo, phone), receipt number (REC-YYYY-NNNN), date, order number, client name, amount, method, cumulative paid, and remaining balance.

---

### User Story 8 - Multi-Artisan Assignment for Mixed Orders (Priority: P2)

As a manager, when handling a mixed-type order (embroidery + beading), I need to assign up to 3 artisans simultaneously: a main tailor, an embroiderer, and a beader. Each artisan sees only the orders in their active phase. The order progresses through the sequential workflow (In Progress -> Embroidery -> Beading -> Ready) with each phase triggering notifications to the relevant artisan.

**Why this priority**: Multi-artisan assignment is specific to embroidered/beaded/mixed work types and extends the core order workflow for the workshop's specialized capabilities.

**Independent Test**: Can be tested by creating a mixed-type order, assigning 3 artisans, and verifying that each artisan sees the order only when their phase is active, and receives the correct notification at phase transition.

**Acceptance Scenarios**:

1. **Given** a mixed-type order, **When** the manager creates it, **Then** they can assign a main tailor, an embroiderer, and a beader.
2. **Given** the order is in "Embroidery" phase, **When** the embroiderer logs in, **Then** they see this order in their list, but the beader does not yet see it.
3. **Given** the embroidery phase is complete and the order moves to "Beading", **When** the transition occurs, **Then** the beader receives notification N07 and now sees the order in their list.

---

### User Story 9 - Offline Mode with Deferred Sync (Priority: P3)

As any workshop user in an area with unreliable internet, I need the application to work fully offline — creating orders, changing statuses, recording payments — and synchronize all changes when connectivity is restored within 5 seconds.

**Why this priority**: Offline support is critical for workshops in areas with poor connectivity but requires significant additional complexity. The application delivers full value in online mode first.

**Independent Test**: Can be tested by going offline, performing order creation and status changes, then reconnecting and verifying all changes sync within 5 seconds and no data is lost.

**Acceptance Scenarios**:

1. **Given** the user is offline, **When** they create an order or change a status, **Then** the change is persisted locally and queued for synchronization.
2. **Given** the user reconnects to the network, **When** sync begins, **Then** all queued changes are sent and synchronized within 5 seconds.
3. **Given** conflicting changes made offline by two users, **When** sync occurs, **Then** the system detects conflicts and presents them to the manager for resolution.

---

### User Story 10 - Reports and Data Export (Priority: P3)

As a manager, I need to export quarterly data in CSV, Excel (.xlsx with formatting), and PDF (complete report with KPIs, charts, and order list). I also need configurable report parameters before export.

**Why this priority**: Export capabilities enhance decision-making but are not essential for day-to-day workshop operations.

**Independent Test**: Can be tested by exporting a quarter's data in each format and verifying file contents match the displayed dashboard data.

**Acceptance Scenarios**:

1. **Given** the quarterly dashboard, **When** the manager clicks "Export CSV", **Then** a CSV file containing all orders for the quarter is downloaded.
2. **Given** the export option, **When** the manager selects "Export Excel", **Then** an .xlsx file with formatted headers and color-coded statuses is generated.
3. **Given** the export option, **When** the manager selects "Export PDF", **Then** a full report with KPI summary, charts, and order table is generated.

---

### Edge Cases

- What happens when a tailor reaches the maximum configurable active order capacity (default: 10)? The system shows a non-blocking warning requiring confirmation to proceed.
- What happens when a delivery date is set earlier than the minimum delay for the work type (e.g., 1 day for Simple, 7 days for Mixed)? The system rejects the date with an explanation of the minimum required delay.
- What happens when a client is created with a duplicate phone number? The system alerts the user about the existing client and offers to select them or confirm duplicate creation.
- What happens when a "Delivered" order has an unpaid balance and someone attempts to modify it later? The order remains locked; only the manager can add a note.
- What happens when SMS sending fails? The system logs the failure (sent/delivered/failed), retries according to configuration, and the manager can view SMS delivery logs.
- What happens when the daily notification job runs outside the configured SMS time window (e.g., 8h-20h)? SMS notifications are queued and sent at the start of the next allowed window; in-app notifications are delivered immediately.
- What happens when two users attempt to change the same order's status simultaneously? The system uses optimistic concurrency — the second user is notified of the conflict and must refresh before retrying.

## Requirements *(mandatory)*

### Functional Requirements

**Order Management (F01)**
- **FR-001**: System MUST allow creating orders through a 3-step form: client selection/creation, work details (type, description, fabric, photos, technical notes), and planning (delivery date, deposit, total price, assigned artisan).
- **FR-002**: System MUST auto-generate unique sequential order numbers in the format CMD-YYYY-NNNN (reset annually, zero-padded to 4 digits).
- **FR-003**: System MUST automatically calculate outstanding balance as total price minus sum of all payments.
- **FR-004**: System MUST allow order modification (delivery date, price, deposit, notes, assigned artisan, photos) for any order not in "Delivered" status.
- **FR-005**: System MUST log every modification in an audit trail recording who, what, when, on which entity, before value, and after value.
- **FR-006**: System MUST support searching orders by order number, client number, and client name, with combinable filters for status, work type, artisan, and date range.
- **FR-007**: System MUST paginate order lists at 20 items per page.

**Status Lifecycle (F02)**
- **FR-008**: System MUST enforce the defined status transition rules: Received -> Waiting/In Progress; In Progress -> Embroidery (if embroidered/mixed), Beading (if beaded/mixed), Alteration, Ready; Embroidery -> Beading (if mixed), Alteration, Ready; Beading -> Alteration, Ready; Alteration -> In Progress/Embroidery/Beading/Ready; Ready -> Delivered.
- **FR-009**: System MUST require an assigned artisan before transitioning to "In Progress".
- **FR-010**: System MUST require a mandatory alteration reason before transitioning to "Alteration".
- **FR-011**: System MUST require either a zero balance or explicit manager validation (with reason) plus actual delivery date to transition to "Delivered".
- **FR-012**: System MUST record every status transition in a timeline with: previous status, new status, user, timestamp, and optional reason.
- **FR-013**: System MUST calculate and display the duration spent in each status on the order timeline.

**Work Types (F03)**
- **FR-014**: System MUST support 4 work types: Simple (standard workflow), Embroidered (adds embroidery phase), Beaded (adds beading phase), and Mixed (adds both embroidery and beading phases sequentially).
- **FR-015**: System MUST display additional fields for embroidered orders (embroidery style, thread colors, density, embroidery zone) and beaded orders (bead type, arrangement, affected zones).
- **FR-016**: System MUST support assigning up to 3 artisans simultaneously on a mixed order: main tailor, embroiderer, and beader.
- **FR-017**: Each artisan MUST only see orders that are currently in their active phase.

**Dashboard (F04)**
- **FR-018**: System MUST display 8 KPI cards on the dashboard: total orders, delivered orders, late orders, on-time delivery rate, revenue collected, outstanding balances, embroidered order count, and beaded order count.
- **FR-019**: System MUST provide quarter navigation (previous/next arrows and dropdown) with the current quarter selected by default.
- **FR-020**: System MUST display 5 analytics charts: monthly histogram by work type, status donut, work type donut, revenue trend (4 quarters), and average delay by artisan (top 5).
- **FR-021**: System MUST support export of quarterly data in CSV, Excel (.xlsx with formatting), and PDF (complete report).

**Notifications (F05)**
- **FR-022**: System MUST run a daily background job to evaluate all active orders for overdue deliveries (N01), upcoming deadlines at 24h (N02) and 48h (N03), and stalled orders exceeding configurable thresholds (N04).
- **FR-023**: System MUST trigger event-based notifications on status transitions: alteration (N05), ready (N06), artisan assignment (N07), and delivery with unpaid balance (N08).
- **FR-024**: System MUST provide an in-app notification center with unread count badge, priority icons, tabs (All/Unread/Critical), mark-as-read, and direct links to related orders.
- **FR-025**: System MUST send SMS for high-priority notifications (N01, N02, N05, N07) within configurable time windows only, with delivery tracking (sent/delivered/failed).
- **FR-026**: System MUST retain notifications for 30 days.
- **FR-027**: System MUST allow the manager to enable/disable each notification type independently, configure thresholds per work type, choose channels, and set SMS time windows.

**Client Management (F06)**
- **FR-028**: System MUST auto-generate unique sequential client numbers in the format C-NNNN (never reset).
- **FR-029**: System MUST store client data: name, surname, primary phone (Algerian format: 05/06/07XXXXXXXX), optional secondary phone, optional address, optional date of birth, optional notes.
- **FR-030**: System MUST provide a fully customizable measurement grid with default fields (bust, waist, hips, dress length, skirt length, sleeve length, arm circumference, shoulder, back width, total height) and support for manager-defined custom fields.
- **FR-031**: System MUST track measurement history with timestamps for each modification.
- **FR-032**: System MUST support client search by number (exact), name/surname (partial, case and diacritic insensitive), and phone number (partial) with real-time results (debounced 300ms).
- **FR-033**: System MUST detect potential duplicate clients by matching phone numbers and alert the user.

**Users and Roles (F07)**
- **FR-034**: System MUST support 5 roles: Manager (full access), Tailor (own assigned orders), Embroiderer (orders in embroidery phase), Beader (orders in beading phase), Cashier (financial module and receipts).
- **FR-035**: System MUST support multi-role assignment (e.g., a user can be both Tailor and Embroiderer).
- **FR-036**: System MUST authenticate users via username and password with optional SMS-based OTP (2FA).
- **FR-037**: System MUST support configurable session duration (8h/24h/7 days) with automatic logout on inactivity.

**Financial Tracking (F08)**
- **FR-038**: System MUST support recording multiple partial payments per order with amount, payment method (Cash/Transfer/CCP/BaridiMob/Dahabia), date, and optional note.
- **FR-039**: System MUST generate a PDF receipt for each payment with workshop header, receipt number (REC-YYYY-NNNN), date, order number, client name, amount, method, cumulative paid, and remaining balance.
- **FR-040**: System MUST provide a financial dashboard view showing quarterly revenue by payment method, outstanding balances, and delivered orders with unpaid amounts.

**Business Rules**
- **FR-041**: System MUST enforce minimum delivery date based on work type: Simple = 1 business day, Embroidered = 3 business days, Beaded = 5 business days, Mixed = 7 business days from reception date.
- **FR-042**: System MUST warn (non-blocking, confirmation required) when assigning an order to a tailor who has reached the configurable maximum active orders (default: 10).
- **FR-043**: System MUST calculate delay in business days: MAX(0, business_days(today - expected_delivery_date)).
- **FR-044**: System MUST support complete audit trail for all critical actions: order creation, status changes, price modifications, payments, and client creation.

**Cross-Platform & Offline (F12)**
- **FR-045**: System MUST function on mobile (iOS & Android) and web (modern browsers).
- **FR-046**: System MUST support full offline operation with deferred synchronization upon network restoration.

### Key Entities

- **Order (Commande)**: Central entity representing a couture job. Key attributes: unique code (CMD-YYYY-NNNN), work type (Simple/Embroidered/Beaded/Mixed), status, reception date, expected delivery date, actual delivery date, total price, description, fabric, technical notes, photos. Belongs to one Client, assigned to one or more Artisans.
- **Client**: Workshop customer. Key attributes: unique code (C-NNNN), name, surname, phone(s), address, date of birth, notes. Has many Orders and a Measurement history.
- **Measurement (Mensuration)**: Body measurement record for a Client. Key attributes: measurement type (from configurable grid), value, unit, measurement date. Tracks historical changes.
- **User (Utilisateur)**: Workshop staff member. Key attributes: name, phone, role(s), active/inactive status. Can be assigned to Orders as artisan (tailor, embroiderer, beader).
- **StatusTransition**: Record of an order's status change. Key attributes: previous status, new status, user who made the change, timestamp, optional reason.
- **Payment (Paiement)**: Financial transaction on an order. Key attributes: amount, payment method, date, note. Generates a Receipt.
- **Receipt (Recu)**: PDF document for a payment. Key attributes: unique code (REC-YYYY-NNNN), date, amount, payment method, cumulative paid, remaining balance.
- **Notification**: Alert sent to a user. Key attributes: type (N01-N08), priority, channel (in-app/SMS), read status, related order, creation timestamp.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Workshop staff can create a complete order (with client, work details, and planning) in under 3 minutes.
- **SC-002**: The quarterly dashboard loads and displays all KPI cards and charts within 2 seconds.
- **SC-003**: Client search returns matching results within 500 milliseconds of the user stopping typing.
- **SC-004**: 100% of overdue orders trigger a notification to the relevant artisan and manager within 24 hours of the deadline passing.
- **SC-005**: Stalled orders (no status change beyond configurable threshold) are detected and flagged to the manager within 24 hours of exceeding the threshold.
- **SC-006**: 95% of on-time delivery rate is achieved within 6 months of deployment (baseline: measure current paper-based on-time rate).
- **SC-007**: Outstanding payment balances are visible in real-time with zero discrepancy between recorded payments and displayed balance.
- **SC-008**: The system supports at least 20 concurrent workshop users without performance degradation.
- **SC-009**: All critical actions (order creation, status changes, payments) have a complete audit trail that is queryable by date range and user.
- **SC-010**: When offline, users can perform all core operations (create orders, change statuses, record payments) and sync successfully within 5 seconds of reconnection.
- **SC-011**: The application functions on both mobile devices (iOS and Android) and web browsers with consistent user experience.
- **SC-012**: SMS notifications are delivered within the configured time window with at least 95% delivery success rate.

## Assumptions

- The workshop operates in Algeria with French as the primary language and DZD as the currency. Arabic support is planned for a future release.
- Date format follows JJ/MM/AAAA (dd/MM/yyyy) convention.
- Phone numbers follow Algerian format (05/06/07 followed by 8 digits).
- "Business days" excludes Fridays and Saturdays (Algerian weekend) and national holidays.
- The SMS gateway provider will be configured at deployment time (Twilio, Vonage, or local provider).
- Photo uploads for reference images are limited to common image formats with a reasonable size limit per order.
- The configurable measurement grid ships with a default set of 10 standard measurements but can be extended by the manager without developer intervention.
- The workshop logo and header information for PDF receipts are configured once during initial setup.
- Excel import of historical data (M09) is explicitly out of scope for v1.
