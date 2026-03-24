# Users & Auth API Contract

**Module**: Users (F07) | **Base Path**: `/api/users`, `/api/auth`

## Authentication

### POST /api/auth/login
**Login with credentials**

- **Auth**: Public
- **Request**: `{ username, password }`
- **Response 200**: `{ userId, username, fullName, roles[], requiresOtp: bool }`
  - If `requiresOtp = true`, client must call `/api/auth/verify-otp`
  - If `requiresOtp = false`, session cookie is set
- **Response 401**: Invalid credentials

---

### POST /api/auth/verify-otp
**Verify SMS OTP for 2FA**

- **Auth**: Partially authenticated (post-login, pre-OTP)
- **Request**: `{ otp: string }`
- **Response 200**: `{ userId, username, fullName, roles[] }` + session cookie
- **Response 401**: Invalid or expired OTP

---

### POST /api/auth/logout
**Logout and invalidate session**

- **Auth**: Any authenticated user
- **Response 204**: Session invalidated

---

### GET /api/auth/me
**Get current user profile**

- **Auth**: Any authenticated user
- **Response 200**: `{ userId, username, firstName, lastName, phone, roles[], twoFactorEnabled, sessionDuration, lastLoginAt }`

---

## User Management

### GET /api/users
**List all users**

- **Auth**: Manager
- **Query params**: `role`, `isActive`, `page`, `pageSize`
- **Response 200**: `{ items: UserSummary[], totalCount }`
  - `UserSummary`: `{ id, username, fullName, phone, roles[], isActive, activeOrderCount, lastLoginAt }`

---

### POST /api/users
**Create new user**

- **Auth**: Manager
- **Request**: `CreateUserCommand`
  - `username`: string (required, unique)
  - `password`: string (required, min 8 chars)
  - `firstName`: string (required)
  - `lastName`: string (required)
  - `phone`: string (required)
  - `roles`: string[] (required, at least one)
  - `twoFactorEnabled`: bool (optional, default false)
  - `sessionDuration`: string (optional, 8h/24h/7days, default 8h)
- **Response 201**: `{ userId, username }`
- **Response 400**: Validation errors
- **Response 409**: Username already exists

---

### PUT /api/users/{id}
**Update user**

- **Auth**: Manager
- **Request**: Partial update
- **Response 200**: Updated UserSummary

---

### PUT /api/users/{id}/password
**Reset user password**

- **Auth**: Manager (for any user), or authenticated user (for self)
- **Request**: `{ newPassword }` (manager) or `{ currentPassword, newPassword }` (self)
- **Response 204**: Password updated

---

### PUT /api/users/{id}/status
**Activate/deactivate user**

- **Auth**: Manager
- **Request**: `{ isActive: bool }`
- **Response 204**: Status updated

---

### GET /api/users/artisans
**List available artisans (for order assignment)**

- **Auth**: Manager, Tailor
- **Query params**: `role` (tailor/embroiderer/beader), `availableOnly` (exclude those at max capacity)
- **Response 200**: `Artisan[]`
  - `{ id, fullName, roles[], activeOrderCount, maxOrders, isAtCapacity }`
