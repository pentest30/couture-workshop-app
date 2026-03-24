# Orders API Contract

**Module**: Orders (F01, F02, F03) | **Base Path**: `/api/orders`

## Endpoints

### POST /api/orders
**Create a new order** (3-step form submission)

- **Auth**: Manager, Tailor
- **Request**: `CreateOrderCommand`
  - `clientId`: ClientId (required)
  - `workType`: string enum (SIMPLE, BRODE, PERLE, MIXTE) (required)
  - `description`: string (optional, max 2000)
  - `fabric`: string (optional, max 500)
  - `technicalNotes`: string (optional, max 2000)
  - `expectedDeliveryDate`: date (required, must respect min delay per work type)
  - `totalPrice`: decimal (required, > 0)
  - `initialDeposit`: decimal (optional, >= 0, <= totalPrice)
  - `depositPaymentMethod`: string enum (optional, required if initialDeposit > 0)
  - `assignedTailorId`: UserId (optional)
  - `assignedEmbroidererId`: UserId (optional, only for BRODE/MIXTE)
  - `assignedBeaderId`: UserId (optional, only for PERLE/MIXTE)
  - `embroideryStyle`: string (optional, for BRODE/MIXTE)
  - `threadColors`: string (optional, for BRODE/MIXTE)
  - `density`: string (optional, for BRODE/MIXTE)
  - `embroideryZone`: string (optional, for BRODE/MIXTE)
  - `beadType`: string (optional, for PERLE/MIXTE)
  - `arrangement`: string (optional, for PERLE/MIXTE)
  - `affectedZones`: string (optional, for PERLE/MIXTE)
  - `photos`: file[] (optional, max 10, image formats only)
- **Response 201**: `{ orderId, code, status, outstandingBalance, receiptUrl? }`
- **Response 400**: Validation errors (invalid date, missing fields, capacity warning)
- **Response 403**: Unauthorized role

---

### GET /api/orders
**List orders with filters and pagination**

- **Auth**: Manager (all), Tailor/Embroiderer/Beader (assigned only), Cashier (limited view)
- **Query params**:
  - `search`: string (order code, client code, client name)
  - `status`: string[] (filter by status codes)
  - `workType`: string[] (filter by work type)
  - `artisanId`: UserId (filter by assigned artisan)
  - `dateFrom`, `dateTo`: date (reception date range)
  - `lateOnly`: bool (only overdue orders)
  - `quarter`: string (e.g., "2026-Q1" — for dashboard view)
  - `page`: int (default 1)
  - `pageSize`: int (default 20, max 100)
  - `sortBy`: string (expectedDeliveryDate, createdAt, status)
  - `sortDir`: string (asc, desc)
- **Response 200**: `{ items: OrderSummary[], totalCount, page, pageSize }`
  - `OrderSummary`: `{ id, code, clientCode, clientName, workType, expectedDeliveryDate, status, delayDays, assignedTailorName, outstandingBalance }`

---

### GET /api/orders/{id}
**Get full order detail**

- **Auth**: Manager, assigned artisan, Cashier
- **Response 200**: `OrderDetail`
  - All OrderSummary fields plus: description, fabric, technicalNotes, receptionDate, actualDeliveryDate, totalPrice, photos[], timeline[], payments[], notifications[], assignedEmbroidererId, assignedBeaderId, embroidery/beading-specific fields
- **Response 404**: Order not found

---

### PUT /api/orders/{id}
**Update order (if not LIVREE)**

- **Auth**: Manager, Tailor (limited fields)
- **Request**: `UpdateOrderCommand` (partial — only changed fields)
  - Editable: expectedDeliveryDate, totalPrice, technicalNotes, assignedTailorId, assignedEmbroidererId, assignedBeaderId, photos
- **Response 200**: Updated OrderDetail
- **Response 400**: Validation errors
- **Response 409**: Order is in LIVREE status (immutable)

---

### POST /api/orders/{id}/status
**Change order status**

- **Auth**: Manager (all transitions), Tailor/Embroiderer/Beader (limited transitions)
- **Request**: `ChangeStatusCommand`
  - `newStatus`: string enum (required)
  - `reason`: string (required for RETOUCHE, delivery with unpaid balance)
  - `assignedEmbroidererId`: UserId (required for → BRODERIE)
  - `assignedBeaderId`: UserId (required for → PERLAGE)
  - `actualDeliveryDate`: date (required for → LIVREE)
- **Response 200**: `{ orderId, previousStatus, newStatus, transitionedAt }`
- **Response 400**: Invalid transition, missing required fields
- **Response 403**: Role not authorized for this transition

---

### GET /api/orders/{id}/timeline
**Get status transition timeline**

- **Auth**: Manager, assigned artisan
- **Response 200**: `StatusTransition[]`
  - `{ fromStatus, toStatus, reason?, transitionedBy, transitionedByName, transitionedAt, durationInStatus }`

---

### POST /api/orders/{id}/photos
**Upload reference photos**

- **Auth**: Manager, Tailor
- **Request**: multipart/form-data with image files
- **Response 201**: `{ photoIds[] }`

### DELETE /api/orders/{id}/photos/{photoId}
**Remove a photo**

- **Auth**: Manager
- **Response 204**: Success
