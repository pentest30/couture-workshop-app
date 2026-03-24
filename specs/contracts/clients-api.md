# Clients API Contract

**Module**: Clients (F06) | **Base Path**: `/api/clients`

## Endpoints

### POST /api/clients
**Create a new client**

- **Auth**: Manager, Tailor, Cashier
- **Request**: `CreateClientCommand`
  - `firstName`: string (required, max 100)
  - `lastName`: string (required, max 100)
  - `primaryPhone`: string (required, Algerian format)
  - `secondaryPhone`: string (optional)
  - `address`: string (optional, max 500)
  - `dateOfBirth`: date (optional)
  - `notes`: string (optional, max 2000)
  - `measurements`: `{ measurementFieldId, value }[]` (optional)
- **Response 201**: `{ clientId, code }`
- **Response 400**: Validation errors
- **Response 409**: Duplicate phone warning `{ existingClient: { id, code, name } }`

---

### GET /api/clients
**List clients with pagination**

- **Auth**: Manager, Tailor (read-only), Cashier
- **Query params**:
  - `search`: string (name, code, phone — partial, diacritic-insensitive)
  - `page`: int (default 1)
  - `pageSize`: int (default 20)
- **Response 200**: `{ items: ClientSummary[], totalCount }`
  - `ClientSummary`: `{ id, code, firstName, lastName, primaryPhone, activeOrderCount }`

---

### GET /api/clients/search
**Real-time search (debounced)**

- **Auth**: Manager, Tailor, Cashier
- **Query params**:
  - `q`: string (min 2 chars)
- **Response 200**: `ClientSummary[]` (max 10 results)

---

### GET /api/clients/{id}
**Full client detail**

- **Auth**: Manager, Tailor, Cashier
- **Response 200**: `ClientDetail`
  - All ClientSummary fields plus: secondaryPhone, address, dateOfBirth, notes, measurements (current), stats: `{ totalOrders, totalAmountCollected, lastVisitDate, activeOrders }`
- **Response 404**: Client not found

---

### PUT /api/clients/{id}
**Update client info**

- **Auth**: Manager, Tailor
- **Request**: Partial update (only changed fields)
- **Response 200**: Updated ClientDetail

---

### GET /api/clients/{id}/measurements
**Get measurement history**

- **Auth**: Manager, Tailor
- **Response 200**: `{ current: Measurement[], history: MeasurementHistoryEntry[] }`
  - `Measurement`: `{ fieldId, fieldName, unit, value, measuredAt, measuredBy }`
  - `MeasurementHistoryEntry`: `{ fieldName, oldValue, newValue, changedAt, changedBy }`

---

### POST /api/clients/{id}/measurements
**Record new measurements**

- **Auth**: Manager, Tailor
- **Request**: `{ measurements: { measurementFieldId, value }[] }`
- **Response 200**: Updated current measurements

---

### GET /api/clients/{id}/orders
**Client order history**

- **Auth**: Manager, Tailor, Cashier
- **Query params**: `page`, `pageSize`
- **Response 200**: `{ items: OrderSummary[], totalCount }`

---

## Measurement Fields (Admin)

### GET /api/measurement-fields
**List all measurement fields**

- **Auth**: Manager
- **Response 200**: `MeasurementField[]`

### POST /api/measurement-fields
**Add custom measurement field**

- **Auth**: Manager
- **Request**: `{ name, unit, displayOrder }`
- **Response 201**: `{ id, name, unit }`

### PUT /api/measurement-fields/{id}
**Update field**

- **Auth**: Manager

### DELETE /api/measurement-fields/{id}
**Deactivate field (soft delete)**

- **Auth**: Manager
