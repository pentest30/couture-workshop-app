# Finance API Contract

**Module**: Finance (F08) | **Base Path**: `/api/finance`

## Payments

### POST /api/orders/{orderId}/payments
**Record a payment on an order**

- **Auth**: Manager, Cashier
- **Request**: `RecordPaymentCommand`
  - `amount`: decimal (required, > 0, <= outstanding balance)
  - `paymentMethod`: string enum (ESPECES, VIREMENT, CCP, BARIDIMOB, DAHABIA) (required)
  - `paymentDate`: date (required)
  - `note`: string (optional, max 500)
- **Response 201**: `{ paymentId, receiptId, receiptCode, receiptUrl, newOutstandingBalance }`
- **Response 400**: Amount exceeds outstanding balance, validation errors
- **Response 403**: Unauthorized role

---

### GET /api/orders/{orderId}/payments
**List payments for an order**

- **Auth**: Manager, Cashier, assigned artisan
- **Response 200**: `Payment[]`
  - `{ id, amount, paymentMethod, paymentDate, note, recordedBy, recordedByName, receiptCode, createdAt }`

---

### GET /api/finance/receipts/{receiptId}/pdf
**Download receipt PDF**

- **Auth**: Manager, Cashier
- **Response 200**: PDF file (`application/pdf`)
- **Response 404**: Receipt not found

---

## Financial Dashboard

### GET /api/finance/summary
**Quarterly financial summary**

- **Auth**: Manager, Cashier
- **Query params**: `year`, `quarter`
- **Response 200**: `FinancialSummary`
  - `totalRevenue`: decimal (DZD)
  - `revenueByMethod`: `[{ method, amount, percentage }]`
  - `outstandingBalances`: decimal (DZD)
  - `outstandingOrderCount`: int
  - `deliveredWithUnpaid`: `[{ orderId, orderCode, clientName, outstandingAmount }]`

---

## Workshop Settings

### GET /api/settings
**Get workshop settings**

- **Auth**: Manager
- **Response 200**: `WorkshopSettings`
  - `{ workshopName, address, phone, logoUrl, maxActiveOrdersPerTailor, defaultSessionDuration }`

---

### PUT /api/settings
**Update workshop settings**

- **Auth**: Manager
- **Request**: Partial update
- **Response 200**: Updated settings

---

### POST /api/settings/logo
**Upload workshop logo**

- **Auth**: Manager
- **Request**: multipart/form-data (image file)
- **Response 200**: `{ logoUrl }`

---

## Holidays (for business day calculation)

### GET /api/settings/holidays
**List configured holidays**

- **Auth**: Manager
- **Query params**: `year` (optional)
- **Response 200**: `Holiday[]`

### POST /api/settings/holidays
**Add a holiday**

- **Auth**: Manager
- **Request**: `{ date, name, isRecurring }`
- **Response 201**: `{ id }`

### DELETE /api/settings/holidays/{id}
**Remove a holiday**

- **Auth**: Manager
- **Response 204**: Success
