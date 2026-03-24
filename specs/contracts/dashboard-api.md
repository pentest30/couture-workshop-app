# Dashboard API Contract

**Module**: Dashboard (F04) | **Base Path**: `/api/dashboard`

## Endpoints

### GET /api/dashboard/kpis
**Get quarterly KPI cards**

- **Auth**: Manager, Cashier (finance KPIs only)
- **Query params**:
  - `year`: int (required)
  - `quarter`: int (1-4, required)
- **Response 200**: `QuarterlyKPIs`
  - `totalOrders`: int
  - `totalOrdersDelta`: decimal (% vs previous quarter)
  - `deliveredOrders`: int
  - `lateOrders`: int
  - `onTimeDeliveryRate`: decimal (%)
  - `revenueCollected`: decimal (DZD)
  - `outstandingBalances`: decimal (DZD)
  - `embroideredOrders`: int
  - `beadedOrders`: int
  - `quarter`: string (e.g., "T1 2026")

---

### GET /api/dashboard/charts/monthly-histogram
**Monthly order count by work type**

- **Auth**: Manager
- **Query params**: `year`, `quarter`
- **Response 200**: `{ months: [{ month, simple, embroidered, beaded, mixed }] }`

---

### GET /api/dashboard/charts/status-distribution
**Current status distribution (donut)**

- **Auth**: Manager
- **Query params**: `year`, `quarter`
- **Response 200**: `{ statuses: [{ status, count, percentage }] }`

---

### GET /api/dashboard/charts/worktype-distribution
**Work type distribution (donut)**

- **Auth**: Manager
- **Query params**: `year`, `quarter`
- **Response 200**: `{ workTypes: [{ type, count, percentage }] }`

---

### GET /api/dashboard/charts/revenue-trend
**Revenue over last 4 quarters (line)**

- **Auth**: Manager, Cashier
- **Query params**: `year`, `quarter` (current reference point)
- **Response 200**: `{ quarters: [{ label, revenue }] }` (4 entries)

---

### GET /api/dashboard/charts/delay-by-artisan
**Average delay by artisan — top 5 (bar)**

- **Auth**: Manager
- **Query params**: `year`, `quarter`
- **Response 200**: `{ artisans: [{ name, avgDelayDays }] }` (max 5)

---

### GET /api/dashboard/export
**Export quarterly report**

- **Auth**: Manager, Cashier (finance only)
- **Query params**:
  - `year`, `quarter`
  - `format`: string (csv, xlsx, pdf)
- **Response 200**: File download with appropriate Content-Type
  - CSV: `text/csv`
  - XLSX: `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
  - PDF: `application/pdf`
