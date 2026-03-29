const BASE = '/api';

let token: string | null = typeof window !== 'undefined' ? localStorage.getItem('token') : null;

export function setToken(t: string) {
  token = t;
  if (typeof window !== 'undefined') localStorage.setItem('token', t);
}

export function clearToken() {
  token = null;
  if (typeof window !== 'undefined') localStorage.removeItem('token');
}

export function getToken() { return token; }

// File upload (multipart)
export async function uploadFile(file: File): Promise<{ fileName: string; storagePath: string; size: number }> {
  const form = new FormData();
  form.append('file', file);
  const res = await fetch(`${BASE}/uploads`, {
    method: 'POST',
    headers: token ? { 'Authorization': `Bearer ${token}` } : {},
    body: form,
  });
  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error(body.error || `Upload failed: ${res.status}`);
  }
  return res.json();
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers: Record<string, string> = { 'Content-Type': 'application/json', ...options.headers as Record<string, string> };
  if (token) headers['Authorization'] = `Bearer ${token}`;

  const res = await fetch(`${BASE}${path}`, { ...options, headers });

  if (res.status === 401) {
    clearToken();
    if (typeof window !== 'undefined') window.location.href = '/login';
    throw new Error('Non autorisé');
  }

  if (res.status === 204) return undefined as T;

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error(body.error || body.title || `Erreur ${res.status}`);
  }

  return res.json();
}

// Auth
export const auth = {
  login: (email: string, password: string) =>
    request<{ accessToken: string; fullName: string; roles: string[] }>('/auth/login', {
      method: 'POST', body: JSON.stringify({ email, password }),
    }),
};

// Orders
export const orders = {
  list: (params: Record<string, string | number | boolean | undefined>) => {
    const qs = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== '') qs.set(k, String(v)); });
    return request<{ items: OrderSummary[]; totalCount: number; page: number; pageSize: number }>(`/orders?${qs}`);
  },
  get: (id: string) => request<OrderDetail>(`/orders/${id}`),
  create: (data: Record<string, unknown>) => request<{ orderId: string; code: string }>('/orders', { method: 'POST', body: JSON.stringify(data) }),
  changeStatus: (id: string, data: Record<string, unknown>) => request(`/orders/${id}/status`, { method: 'POST', body: JSON.stringify(data) }),
  deactivate: (id: string) => request(`/orders/${id}`, { method: 'DELETE' }),
};

// Clients
export const clients = {
  list: (params: Record<string, string | number | undefined>) => {
    const qs = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== '') qs.set(k, String(v)); });
    return request<{ items: Client[]; totalCount: number }>(`/clients?${qs}`);
  },
  get: (id: string) => request<Client>(`/clients/${id}`),
  search: (q: string) => request<Client[]>(`/clients/search?q=${encodeURIComponent(q)}`),
  create: async (data: Record<string, unknown>) => {
    const headers: Record<string, string> = { 'Content-Type': 'application/json' };
    if (token) headers['Authorization'] = `Bearer ${token}`;
    const res = await fetch(`${BASE}/clients`, { method: 'POST', headers, body: JSON.stringify(data) });
    if (res.status === 409) {
      const body = await res.json();
      if (body.duplicate) throw Object.assign(new Error(body.error), { duplicate: true, ...body });
      throw new Error(body.error || 'Conflit');
    }
    if (!res.ok) { const body = await res.json().catch(() => ({})); throw new Error(body.error || `Erreur ${res.status}`); }
    return res.json() as Promise<Client>;
  },
  update: (id: string, data: Record<string, unknown>) => request(`/clients/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  getMeasurements: (id: string) => request<{ current: Measurement[]; history: unknown[] }>(`/clients/${id}/measurements`),
  saveMeasurements: (id: string, measurements: Measurement[]) =>
    request(`/clients/${id}/measurements`, { method: 'POST', body: JSON.stringify({ measurements }) }),
  deactivate: (id: string) => request(`/clients/${id}`, { method: 'DELETE' }),
};

// Measurement Fields
export const measurementFields = {
  list: () => request<MeasurementField[]>('/measurement-fields'),
  create: (data: { name: string; unit: string; displayOrder: number }) =>
    request<{ id: string }>('/measurement-fields', { method: 'POST', body: JSON.stringify(data) }),
  remove: (id: string) => request(`/measurement-fields/${id}`, { method: 'DELETE' }),
};

// Catalog
export const catalog = {
  listModels: (params: Record<string, string | number | boolean | undefined>) => {
    const qs = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== '') qs.set(k, String(v)); });
    return request<{ items: CatalogModel[]; totalCount: number }>(`/catalog?${qs}`);
  },
  getModel: (id: string) => request<CatalogModelDetail>(`/catalog/${id}`),
  createModel: (data: Record<string, unknown>) => request<{ id: string; name: string }>('/catalog', { method: 'POST', body: JSON.stringify(data) }),
  updateModel: (id: string, data: Record<string, unknown>) => request(`/catalog/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  addPhoto: (modelId: string, data: { fileName: string; storagePath: string; sortOrder: number }) =>
    request<{ photoId: string }>(`/catalog/${modelId}/photos`, { method: 'POST', body: JSON.stringify(data) }),
  removePhoto: (modelId: string, photoId: string) => request(`/catalog/${modelId}/photos/${photoId}`, { method: 'DELETE' }),
  linkFabric: (modelId: string, fabricId: string) => request(`/catalog/${modelId}/fabrics/${fabricId}`, { method: 'POST' }),
  unlinkFabric: (modelId: string, fabricId: string) => request(`/catalog/${modelId}/fabrics/${fabricId}`, { method: 'DELETE' }),
  listFabrics: (params: Record<string, string | number | undefined>) => {
    const qs = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== '') qs.set(k, String(v)); });
    return request<{ items: CatalogFabric[]; totalCount: number }>(`/catalog/fabrics?${qs}`);
  },
  getFabric: (id: string) => request<CatalogFabric>(`/catalog/fabrics/${id}`),
  createFabric: (data: Record<string, unknown>) => request<{ id: string }>('/catalog/fabrics', { method: 'POST', body: JSON.stringify(data) }),
  deactivateModel: (id: string) => request(`/catalog/${id}`, { method: 'DELETE' }),
};

export interface CatalogModel {
  id: string; code: string; name: string; category: string; categoryLabel: string;
  workType: string; basePrice: number; estimatedDays: number; isPublic: boolean;
  primaryPhotoPath?: string; createdAt: string;
}

export interface CatalogModelDetail extends CatalogModel {
  description?: string;
  photos: { id: string; fileName: string; storagePath: string; sortOrder: number; uploadedAt: string }[];
  fabrics: CatalogFabric[];
}

export interface CatalogFabric {
  id: string; name: string; type: string; color: string;
  supplier?: string; pricePerMeter: number; stockMeters: number;
  description?: string; swatchPath?: string;
}

// Dashboard
export const dashboard = {
  kpis: (year: number, quarter: number) =>
    request<KPIs>(`/dashboard/kpis?year=${year}&quarter=${quarter}`),
  monthlyHistogram: (year: number, quarter: number) =>
    request<{ months: ChartMonth[] }>(`/dashboard/charts/monthly-histogram?year=${year}&quarter=${quarter}`),
  statusDistribution: (year: number, quarter: number) =>
    request<{ statuses: ChartStatus[] }>(`/dashboard/charts/status-distribution?year=${year}&quarter=${quarter}`),
  revenueTrend: (year: number, quarter: number) =>
    request<{ quarters: ChartRevenue[] }>(`/dashboard/charts/revenue-trend?year=${year}&quarter=${quarter}`),
  delayByArtisan: (year: number, quarter: number) =>
    request<{ artisans: ArtisanDelay[] }>(`/dashboard/charts/delay-by-artisan?year=${year}&quarter=${quarter}`),
  exportUrl: (year: number, quarter: number, format: 'csv' | 'pdf') =>
    `${BASE}/dashboard/export?year=${year}&quarter=${quarter}&format=${format}`,
};

// Finance
export const finance = {
  summary: (year: number, quarter: number) =>
    request<FinanceSummary>(`/finance/summary?year=${year}&quarter=${quarter}`),
  getPayments: (orderId: string) => request<Payment[]>(`/orders/${orderId}/payments`),
  recordPayment: (orderId: string, data: Record<string, unknown>) =>
    request<{ paymentId: string; receiptCode: string; outstanding: number }>(`/orders/${orderId}/payments`, { method: 'POST', body: JSON.stringify(data) }),
  receiptUrl: (paymentId: string) => `${BASE}/finance/receipts/${paymentId}/pdf`,
};

// Users
export const users = {
  list: (params?: { search?: string; activeOnly?: boolean; role?: string }) => {
    const qs = new URLSearchParams();
    if (params?.search) qs.set('search', params.search);
    if (params?.activeOnly) qs.set('activeOnly', 'true');
    if (params?.role) qs.set('role', params.role);
    return request<{ items: User[]; totalCount: number }>(`/users?${qs}`);
  },
  get: (id: string) => request<User>(`/users/${id}`),
  create: (data: Record<string, unknown>) => request<{ id: string }>('/users', { method: 'POST', body: JSON.stringify(data) }),
  update: (id: string, data: Record<string, unknown>) => request(`/users/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  deactivate: (id: string) => request(`/users/${id}`, { method: 'DELETE' }),
};

// Notifications
export const notifications = {
  list: (filter = 'unread', page = 1) =>
    request<{ items: Notification[]; totalCount: number; unreadCount: number }>(`/notifications?filter=${filter}&page=${page}`),
  unreadCount: () => request<{ count: number }>('/notifications/unread-count'),
  markRead: (id: string) => request(`/notifications/${id}/read`, { method: 'POST' }),
  markAllRead: () => request('/notifications/read-all', { method: 'POST' }),
  getConfigs: () => request<NotifConfig[]>('/notifications/admin/configs'),
  updateConfig: (typeValue: number, data: Record<string, unknown>) =>
    request(`/notifications/admin/config/${typeValue}`, { method: 'PUT', body: JSON.stringify(data) }),
};

// Types
export interface OrderSummary {
  id: string; code: string; clientId: string; clientName: string;
  status: string; statusLabel: string; statusColor: string;
  workType: string; workTypeLabel: string;
  expectedDeliveryDate: string; delayDays: number; isLate: boolean;
  totalPrice: number; outstandingBalance: number;
  assignedTailorId?: string; createdAt: string;
}

export interface OrderDetail extends OrderSummary {
  description?: string; fabric?: string; technicalNotes?: string;
  embroideryStyle?: string; threadColors?: string; density?: string; embroideryZone?: string;
  beadType?: string; arrangement?: string; affectedZones?: string;
  receptionDate: string; actualDeliveryDate?: string;
  assignedTailorName?: string;
  assignedEmbroidererId?: string; assignedEmbroidererName?: string;
  assignedBeaderId?: string; assignedBeaderName?: string;
  hasUnpaidBalance: boolean;
  timeline: TimelineEntry[]; photos: Photo[];
}

export interface TimelineEntry {
  fromStatus?: string; toStatus: string; toStatusLabel: string; toStatusColor: string;
  reason?: string; transitionedBy: string; transitionedAt: string; duration?: string;
}

export interface Photo { id: string; fileName: string; storagePath: string; uploadedAt: string; }

export interface Client {
  id: string; code: string; firstName: string; lastName: string;
  primaryPhone: string; secondaryPhone?: string; address?: string;
  dateOfBirth?: string; notes?: string;
  fullName?: string;
}

export interface Measurement { fieldName: string; value: number; unit: string; }
export interface MeasurementField { id: string; name: string; unit: string; displayOrder: number; isDefault: boolean; }

export interface Payment {
  id: string; orderId: string; amount: number;
  paymentMethod: string; paymentMethodLabel: string;
  paymentDate: string; note?: string; receiptCode?: string;
  recordedBy: string; createdAt: string;
}

export interface KPIs {
  totalOrders: number; totalOrdersDelta: number; deliveredOrders: number;
  lateOrders: number; onTimeDeliveryRate: number;
  revenueCollected: number; outstandingBalances: number;
  embroideredOrders: number; beadedOrders: number; quarter: string;
}

export interface ChartMonth { month: string; simple: number; brode: number; perle: number; mixte: number; }
export interface ChartStatus { status: string; label: string; count: number; color: string; }
export interface ChartRevenue { label: string; revenue: number; }
export interface ArtisanDelay { name: string; avgDelayDays: number; }

export interface FinanceSummary {
  totalCollected: number;
  outstandingBalances: number;
  paymentsByMethod: { method: string; label: string; total: number }[];
  recentPayments: Payment[];
}

export interface Notification {
  id: string; type: string; typeLabel: string; priority: string;
  title: string; message: string; orderId: string;
  isRead: boolean; createdAt: string; readAt?: string;
}

export interface User {
  id: string; userName: string; firstName: string; lastName: string;
  email: string; phone?: string; roles: string[];
  isActive: boolean; lastLoginAt?: string; createdAt: string;
}

export const AVAILABLE_ROLES = ['Manager', 'Tailor', 'Embroiderer', 'Beader', 'Cashier'] as const;
export const ROLE_LABELS: Record<string, string> = {
  Manager: 'Gérant', Tailor: 'Couturière', Embroiderer: 'Brodeur(se)',
  Beader: 'Perleur(se)', Cashier: 'Caissier(ère)',
};

export interface NotifConfig {
  typeValue: number; typeName: string; typeLabel: string; priority: string;
  isEnabled: boolean; smsEnabled: boolean;
  stallThresholdSimple: number; stallThresholdEmbroidered: number;
  stallThresholdBeaded: number; stallThresholdMixed: number;
  smsWindowStart: string; smsWindowEnd: string;
}
