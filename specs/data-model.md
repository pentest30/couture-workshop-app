# Data Model: Atelier Couture Workshop App

**Feature**: 002-couture-workshop-app | **Date**: 2026-03-24

## Entity Relationship Overview

```text
┌──────────┐     ┌───────────────┐     ┌──────────────┐
│  Client   │1───*│    Order      │*───1│    User      │
│  (C-NNNN) │     │ (CMD-YYYY-N) │     │ (artisans)   │
└──────────┘     └───────────────┘     └──────────────┘
     │1                │1    │1              │1
     │                 │     │               │
     *│                │*    │*              *│
┌──────────┐  ┌────────────┐ ┌──────────┐ ┌──────────┐
│Measurement│  │ Status     │ │ Payment  │ │UserRole  │
│  History  │  │ Transition │ │(Paiement)│ │(junction)│
└──────────┘  └────────────┘ └──────────┘ └──────────┘
                                  │1
                                  │
                                  1│
                             ┌──────────┐
                             │ Receipt  │
                             │(REC-YYYY)│
                             └──────────┘

┌──────────────┐     ┌──────────────────┐
│ Notification │*───1│     Order        │
└──────────────┘     └──────────────────┘
       *│
        │
       1│
  ┌──────────┐
  │   User   │
  └──────────┘

┌──────────────────┐     ┌──────────────────┐
│ MeasurementField │1───*│ ClientMeasurement│
│  (configurable)  │     │   (per client)   │
└──────────────────┘     └──────────────────┘

┌──────────────┐
│  AuditLog    │  (append-only, references any entity)
└──────────────┘

┌──────────────┐
│ Holiday      │  (configurable national holidays)
└──────────────┘

┌────────────────────┐
│ NotificationConfig │  (per-notification-type settings)
└────────────────────┘
```

## Entities

### Client

Central customer entity. Auto-numbered with global sequence.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | ClientId (strongly-typed) | Auto | Internal PK |
| Code | string | Auto | Format: `C-NNNN`, sequential, never reset |
| FirstName | string(100) | Yes | |
| LastName | string(100) | Yes | |
| PrimaryPhone | string(20) | Yes | Algerian format: 05/06/07XXXXXXXX |
| SecondaryPhone | string(20) | No | |
| Address | string(500) | No | |
| DateOfBirth | DateOnly | No | For VIP birthday tracking |
| Notes | string(2000) | No | Preferences, habits |
| CreatedAt | DateTimeOffset | Auto | UTC |
| CreatedBy | UserId | Auto | |

**Validation rules**:
- PrimaryPhone must match pattern `^0[567]\d{8}$`
- FirstName + LastName: non-empty, max 100 chars each
- Code: auto-generated, globally unique, never recycled

**Relationships**:
- Has many Orders
- Has many ClientMeasurements (via MeasurementField)

---

### MeasurementField

Configurable measurement type defined by the manager. Supports default fields + custom additions.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | MeasurementFieldId | Auto | |
| Name | string(100) | Yes | e.g., "Tour de poitrine" |
| Unit | string(10) | Yes | Default: "cm" |
| DisplayOrder | int | Yes | Controls display sequence |
| IsDefault | bool | Yes | True for system-provided fields |
| IsActive | bool | Yes | Soft-deletable |

**Default fields** (seeded): Tour de poitrine, Tour de taille, Tour de hanches, Longueur robe (dos), Longueur jupe, Longueur manche, Tour de bras, Epaule, Carrure dos, Hauteur totale.

---

### ClientMeasurement

A specific measurement value for a client at a point in time. Historical — new records are added on change, old ones are preserved.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | ClientMeasurementId | Auto | |
| ClientId | ClientId | Yes | FK to Client |
| MeasurementFieldId | MeasurementFieldId | Yes | FK to MeasurementField |
| Value | decimal(6,1) | Yes | e.g., 92.0 |
| MeasuredAt | DateTimeOffset | Auto | When this measurement was taken |
| MeasuredBy | UserId | Yes | Who took the measurement |

**Validation rules**:
- Value must be > 0
- One "current" measurement per client per field (latest by MeasuredAt)

---

### Order (Commande)

Central aggregate. Represents a couture job from reception to delivery.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | OrderId (strongly-typed) | Auto | Internal PK |
| Code | string | Auto | Format: `CMD-YYYY-NNNN`, reset annually |
| ClientId | ClientId | Yes | FK to Client |
| Status | OrderStatus (SmartEnum) | Yes | See status values below |
| WorkType | WorkType (SmartEnum) | Yes | Simple, Embroidered, Beaded, Mixed |
| Description | string(2000) | No | Work description |
| Fabric | string(500) | No | Fabric details |
| TechnicalNotes | string(2000) | No | |
| ReceptionDate | DateOnly | Auto | Date order was created |
| ExpectedDeliveryDate | DateOnly | Yes | Must respect minimum delay per work type |
| ActualDeliveryDate | DateOnly | No | Set when status → Delivered |
| TotalPrice | decimal(12,2) | Yes | In DZD |
| AssignedTailorId | UserId | No | Main tailor (required for EN_COURS) |
| AssignedEmbroidererId | UserId | No | For Embroidered/Mixed types |
| AssignedBeaderId | UserId | No | For Beaded/Mixed types |
| DeliveryWithUnpaidReason | string(500) | No | If delivered with outstanding balance |
| HasUnpaidBalance | bool | Yes | Flag for reporting |
| CreatedAt | DateTimeOffset | Auto | |
| CreatedBy | UserId | Auto | |

**Computed properties** (not stored):
- `OutstandingBalance` = TotalPrice - SUM(Payments.Amount)
- `DelayDays` = MAX(0, BusinessDays(today - ExpectedDeliveryDate)) when status != Delivered
- `IsLate` = Status != Delivered AND ExpectedDeliveryDate < today

**Validation rules**:
- TotalPrice > 0
- ExpectedDeliveryDate >= ReceptionDate + MinDelay(WorkType)
  - Simple: 1 business day
  - Embroidered: 3 business days
  - Beaded: 5 business days
  - Mixed: 7 business days
- AssignedTailorId required before transition to EN_COURS
- AssignedEmbroidererId required for transition to BRODERIE (Embroidered/Mixed)
- AssignedBeaderId required for transition to PERLAGE (Beaded/Mixed)

**Relationships**:
- Belongs to one Client
- Has many StatusTransitions
- Has many Payments
- Has many OrderPhotos
- Assigned to Users (tailor, embroiderer, beader)

---

### OrderStatus (SmartEnum)

| Value | Code | Color | Description |
|-------|------|-------|-------------|
| 1 | RECUE | #1565C0 (blue) | Order received, not yet started |
| 2 | EN_ATTENTE | #F9A825 (yellow) | Blocked: waiting for fabric, model, client validation |
| 3 | EN_COURS | #E65100 (orange) | Active sewing work |
| 4 | BRODERIE | #6A1B9A (purple) | Embroidery phase in progress |
| 5 | PERLAGE | #880E4F (pink) | Beading/crystal phase in progress |
| 6 | RETOUCHE | #C62828 (red) | Returned for alterations |
| 7 | PRETE | #2E7D32 (green) | Completed, awaiting pickup |
| 8 | LIVREE | #424242 (gray) | Delivered, case closed (terminal) |

**Allowed transitions**:

| From | To | Conditions |
|------|----|------------|
| RECUE | EN_ATTENTE | None |
| RECUE | EN_COURS | Tailor assigned |
| EN_ATTENTE | EN_COURS | Tailor assigned |
| EN_COURS | BRODERIE | WorkType = Embroidered or Mixed; embroiderer assigned |
| EN_COURS | PERLAGE | WorkType = Beaded or Mixed; beader assigned |
| EN_COURS | RETOUCHE | Alteration reason required |
| EN_COURS | PRETE | None |
| BRODERIE | PERLAGE | WorkType = Mixed; beader assigned |
| BRODERIE | RETOUCHE | Alteration reason required |
| BRODERIE | PRETE | None |
| PERLAGE | RETOUCHE | Alteration reason required |
| PERLAGE | PRETE | None |
| RETOUCHE | EN_COURS | None |
| RETOUCHE | BRODERIE | WorkType = Embroidered or Mixed |
| RETOUCHE | PERLAGE | WorkType = Beaded or Mixed |
| RETOUCHE | PRETE | None |
| PRETE | LIVREE | (Balance = 0 OR manager validation + reason) AND actual delivery date set |

---

### WorkType (SmartEnum)

| Value | Code | Phases | Min Delay | Stall Threshold |
|-------|------|--------|-----------|-----------------|
| 1 | SIMPLE | EN_COURS → PRETE | 1 biz day | 3 days |
| 2 | BRODE | EN_COURS → BRODERIE → PRETE | 3 biz days | 7 days |
| 3 | PERLE | EN_COURS → PERLAGE → PRETE | 5 biz days | 10 days |
| 4 | MIXTE | EN_COURS → BRODERIE → PERLAGE → PRETE | 7 biz days | 14 days |

**Additional fields per work type**:
- Embroidered: EmbroideryStyle, ThreadColors, Density, EmbroideryZone
- Beaded: BeadType, Arrangement, AffectedZones

---

### OrderPhoto

Reference photos attached to an order.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | OrderPhotoId | Auto | |
| OrderId | OrderId | Yes | FK to Order |
| FileName | string(255) | Yes | |
| StoragePath | string(500) | Yes | Relative path in file storage |
| UploadedAt | DateTimeOffset | Auto | |
| UploadedBy | UserId | Yes | |

---

### StatusTransition

Immutable record of each status change on an order.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | StatusTransitionId | Auto | |
| OrderId | OrderId | Yes | FK to Order |
| FromStatus | OrderStatus | Yes | |
| ToStatus | OrderStatus | Yes | |
| Reason | string(500) | No | Required for RETOUCHE, delivery with unpaid |
| TransitionedBy | UserId | Yes | Who made the change |
| TransitionedAt | DateTimeOffset | Auto | UTC |

**Derived**: Duration in each status = next transition timestamp - this transition timestamp.

---

### Payment (Paiement)

Financial transaction against an order. Append-only.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | PaymentId | Auto | |
| OrderId | OrderId | Yes | FK to Order |
| Amount | decimal(12,2) | Yes | In DZD, must be > 0 |
| PaymentMethod | PaymentMethod (SmartEnum) | Yes | |
| PaymentDate | DateOnly | Yes | |
| Note | string(500) | No | |
| RecordedBy | UserId | Yes | |
| CreatedAt | DateTimeOffset | Auto | |

**Validation rules**:
- Amount > 0
- SUM(Payments.Amount) for an order must not exceed TotalPrice

---

### PaymentMethod (SmartEnum)

| Value | Code | Label |
|-------|------|-------|
| 1 | ESPECES | Especes |
| 2 | VIREMENT | Virement |
| 3 | CCP | CCP |
| 4 | BARIDIMOB | BaridiMob |
| 5 | DAHABIA | Dahabia |

---

### Receipt (Recu)

PDF receipt generated for each payment.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | ReceiptId | Auto | |
| Code | string | Auto | Format: `REC-YYYY-NNNN`, reset annually |
| PaymentId | PaymentId | Yes | FK to Payment |
| PdfStoragePath | string(500) | Yes | |
| GeneratedAt | DateTimeOffset | Auto | |

---

### User (Utilisateur)

Workshop staff member.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | UserId (strongly-typed) | Auto | |
| Username | string(50) | Yes | Unique, used for login |
| PasswordHash | string | Yes | Hashed |
| FirstName | string(100) | Yes | |
| LastName | string(100) | Yes | |
| Phone | string(20) | Yes | For SMS notifications |
| IsActive | bool | Yes | Default: true |
| TwoFactorEnabled | bool | Yes | Default: false |
| SessionDuration | SessionDuration (SmartEnum) | Yes | 8h / 24h / 7days |
| LastLoginAt | DateTimeOffset | No | |
| CreatedAt | DateTimeOffset | Auto | |

**Relationships**:
- Has many UserRoles (junction)
- Has many assigned Orders (as tailor, embroiderer, or beader)

---

### UserRole (Junction)

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| UserId | UserId | Yes | FK to User |
| Role | Role (SmartEnum) | Yes | |

---

### Role (SmartEnum)

| Value | Code | Label |
|-------|------|-------|
| 1 | MANAGER | Gerant |
| 2 | TAILOR | Couturier(ere) |
| 3 | EMBROIDERER | Brodeur(euse) |
| 4 | BEADER | Perleur(euse) |
| 5 | CASHIER | Caissier(ere) |

---

### Notification

Alert sent to a user about an order event.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | NotificationId | Auto | |
| Type | NotificationType (SmartEnum) | Yes | N01-N08 |
| Priority | NotificationPriority (SmartEnum) | Yes | Critical, High, Medium |
| OrderId | OrderId | Yes | FK to Order |
| RecipientId | UserId | Yes | FK to User |
| Title | string(200) | Yes | |
| Message | string(1000) | Yes | |
| Channel | NotificationChannel | Yes | InApp, SMS, Both |
| IsRead | bool | Yes | Default: false |
| SmsStatus | SmsDeliveryStatus | No | Sent, Delivered, Failed, null if InApp only |
| CreatedAt | DateTimeOffset | Auto | |
| ReadAt | DateTimeOffset | No | |
| ExpiresAt | DateTimeOffset | Auto | CreatedAt + 30 days |

**Retention**: Notifications older than 30 days are purged by background job.

---

### NotificationType (SmartEnum)

| Value | Code | Trigger | Priority | Default Channel |
|-------|------|---------|----------|-----------------|
| 1 | N01 | Delivery date passed (+1 day) | Critical | InApp + SMS |
| 2 | N02 | Delivery in 24h | High | InApp + SMS |
| 3 | N03 | Delivery in 48h | Medium | InApp |
| 4 | N04 | Stalled order (no status change > threshold) | High | InApp |
| 5 | N05 | Status changed to RETOUCHE | High | InApp + SMS |
| 6 | N06 | Status changed to PRETE | Medium | InApp |
| 7 | N07 | New order assigned to artisan | Medium | InApp + SMS |
| 8 | N08 | Delivery attempted with unpaid balance | Critical | InApp |

---

### NotificationConfig

Manager-configurable settings per notification type.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | int | Auto | |
| NotificationType | NotificationType | Yes | N01-N08 |
| IsEnabled | bool | Yes | Default: true |
| Channel | NotificationChannel | Yes | InApp, SMS, Both |
| StallThresholdSimple | int | No | Days (default: 3) — for N04 only |
| StallThresholdEmbroidered | int | No | Days (default: 7) — for N04 only |
| StallThresholdBeaded | int | No | Days (default: 10) — for N04 only |
| StallThresholdMixed | int | No | Days (default: 14) — for N04 only |
| SmsWindowStart | TimeOnly | No | Default: 08:00 |
| SmsWindowEnd | TimeOnly | No | Default: 20:00 |

---

### AuditLog

Append-only audit trail for all critical actions.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | long | Auto | |
| UserId | UserId | Yes | Who performed the action |
| Action | string(50) | Yes | e.g., "OrderCreated", "StatusChanged", "PaymentRecorded" |
| EntityType | string(50) | Yes | e.g., "Order", "Client", "Payment" |
| EntityId | string(50) | Yes | The entity's ID |
| Timestamp | DateTimeOffset | Auto | UTC |
| BeforeJson | string | No | JSON snapshot of entity before change |
| AfterJson | string | No | JSON snapshot of entity after change |

---

### Holiday

Configurable national holidays for business day calculation.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | int | Auto | |
| Date | DateOnly | Yes | |
| Name | string(100) | Yes | e.g., "Fete de l'Independance" |
| IsRecurring | bool | Yes | True = same date every year |

---

### WorkshopSettings

Global workshop configuration (singleton).

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | int | Auto | Always 1 |
| WorkshopName | string(200) | Yes | For PDF headers |
| Address | string(500) | No | |
| Phone | string(20) | No | |
| LogoPath | string(500) | No | |
| MaxActiveOrdersPerTailor | int | Yes | Default: 10 |
| DefaultSessionDuration | SessionDuration | Yes | Default: 8h |

## Indexes

| Entity | Index | Type | Purpose |
|--------|-------|------|---------|
| Client | Code | Unique | Lookup by client number |
| Client | PrimaryPhone | Non-unique | Duplicate detection |
| Client | FirstName, LastName (unaccent + trigram) | GIN | Diacritic-insensitive search |
| Order | Code | Unique | Lookup by order number |
| Order | Status | Non-unique | Dashboard filters |
| Order | ClientId | Non-unique | Client order history |
| Order | ExpectedDeliveryDate | Non-unique | Overdue/deadline queries |
| Order | AssignedTailorId | Non-unique | Tailor's order list |
| Order | AssignedEmbroidererId | Non-unique | Embroiderer's order list |
| Order | AssignedBeaderId | Non-unique | Beader's order list |
| Order | ReceptionDate | Non-unique | Quarterly dashboard |
| StatusTransition | OrderId, TransitionedAt | Composite | Timeline display |
| Payment | OrderId | Non-unique | Balance calculation |
| Notification | RecipientId, IsRead | Composite | Unread count badge |
| Notification | ExpiresAt | Non-unique | Purge job |
| AuditLog | EntityType, EntityId | Composite | Audit lookup |
| AuditLog | Timestamp | Non-unique | Date range queries |
