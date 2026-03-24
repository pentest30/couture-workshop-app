# Notifications API Contract

**Module**: Notifications (F05) | **Base Path**: `/api/notifications`

## Endpoints

### GET /api/notifications
**List notifications for current user**

- **Auth**: Any authenticated user
- **Query params**:
  - `filter`: string (all, unread, critical) — default: all
  - `page`: int (default 1)
  - `pageSize`: int (default 20)
- **Response 200**: `{ items: Notification[], totalCount, unreadCount }`
  - `Notification`: `{ id, type, priority, title, message, orderId, orderCode, isRead, createdAt, readAt }`

---

### GET /api/notifications/unread-count
**Get unread notification count (for bell badge)**

- **Auth**: Any authenticated user
- **Response 200**: `{ count: int }`

---

### POST /api/notifications/{id}/read
**Mark notification as read**

- **Auth**: Notification recipient
- **Response 204**: Success

---

### POST /api/notifications/read-all
**Mark all notifications as read**

- **Auth**: Any authenticated user
- **Response 204**: Success

---

### DELETE /api/notifications/{id}
**Dismiss notification**

- **Auth**: Notification recipient
- **Response 204**: Success

---

## Real-Time (SignalR Hub)

### Hub: `/hubs/notifications`

**Server → Client events**:
- `ReceiveNotification(Notification notification)` — pushed when a new notification is created for this user
- `UnreadCountChanged(int count)` — pushed when unread count changes

---

## Notification Settings (Admin)

### GET /api/notifications/config
**Get all notification configuration**

- **Auth**: Manager
- **Response 200**: `NotificationConfig[]`
  - `{ type, isEnabled, channel, stallThresholds: { simple, embroidered, beaded, mixed }, smsWindowStart, smsWindowEnd }`

---

### PUT /api/notifications/config/{type}
**Update notification settings**

- **Auth**: Manager
- **Request**: Partial update of config fields
- **Response 200**: Updated config

---

### POST /api/notifications/test-sms
**Send test SMS**

- **Auth**: Manager
- **Request**: `{ phone: string }`
- **Response 200**: `{ status: "sent" | "failed", message? }`

---

## SMS Logs (Admin)

### GET /api/notifications/sms-logs
**View SMS delivery logs**

- **Auth**: Manager
- **Query params**: `page`, `pageSize`, `dateFrom`, `dateTo`, `status` (sent/delivered/failed)
- **Response 200**: `{ items: SmsLog[], totalCount }`
  - `SmsLog`: `{ id, notificationId, recipientPhone, recipientName, message, status, sentAt, deliveredAt?, error? }`
