import axios from 'axios';

// ── Axios instance pointing to the real ASP.NET Core backend ─────
const axiosClient = axios.create({
  baseURL: 'http://localhost:5033/api',
  headers: { 'Content-Type': 'application/json' },
});

// Safe localStorage wrapper to prevent crashes in private/incognito modes
const storage = (() => {
  const inMemory = {};
  const isSupported = () => {
    try {
      const key = '__test_storage__';
      localStorage.setItem(key, key);
      localStorage.removeItem(key);
      return true;
    } catch (e) {
      return false;
    }
  };
  const supported = isSupported();
  return {
    getItem: (key) => {
      if (supported) {
        try {
          return localStorage.getItem(key);
        } catch {
          return inMemory[key] || null;
        }
      }
      return inMemory[key] || null;
    },
    setItem: (key, value) => {
      if (supported) {
        try {
          localStorage.setItem(key, value);
          return;
        } catch {}
      }
      inMemory[key] = value;
    },
    removeItem: (key) => {
      if (supported) {
        try {
          localStorage.removeItem(key);
          return;
        } catch {}
      }
      delete inMemory[key];
    }
  };
})();

// Attach JWT token automatically to every request
axiosClient.interceptors.request.use((config) => {
  const token = storage.getItem('vp_token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// ── Auth helpers ─────────────────────────────────────────────
const auth = {
  getUser: () => { try { return JSON.parse(storage.getItem('vp_user') || 'null'); } catch { return null; } },
  getToken: () => { try { return storage.getItem('vp_token'); } catch { return null; } },
  setAuth: (user, token) => {
    try {
      storage.setItem('vp_user', JSON.stringify(user));
      storage.setItem('vp_token', token);
    } catch {}
  },
  clearAuth: () => {
    try {
      storage.removeItem('vp_user');
      storage.removeItem('vp_token');
    } catch {}
  },
  isLoggedIn: () => { try { return !!storage.getItem('vp_token'); } catch { return false; } },
};

// ── Navigation ────────────────────────────────────────────────
function navigate(path) { window.location.hash = path; }
function getPath() { return window.location.hash.replace('#', '') || '/login'; }

// ── Formatters ────────────────────────────────────────────────
const formatCurrency = (v) => 'Rs ' + parseFloat(v || 0).toLocaleString('en-IN', { minimumFractionDigits: 2 });
const formatDate = (d) => d ? new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }) : '—';
const formatDateTime = (d) => d ? new Date(d).toLocaleString('en-GB', { day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : '—';

// ── Demo Data ──────────────────────────────────────────────────
const DEMO = {
  stats: { totalStaff: 0, totalVendors: 0, totalParts: 0, lowStockParts: 0, todaySales: 0, monthlyRevenue: 0, pendingCredit: 0, customersToday: 0, totalSalesToday: 0 },
  staff: [], vendors: [], parts: [], customers: [], vehicles: [],
  salesInvoices: [], salesItems: [], purchaseInvoices: [], appointments: [],
  partRequests: [], reviews: [], notifications: [], predictions: [],
};

// ── Helper: extract data from ApiResponse<T> wrapper ─────────
const unwrap = (res) => res.data?.data ?? res.data;
const safe = (promise) => promise.catch(() => ({ data: { data: [] } }));
const safeObj = (promise) => promise.catch(() => ({ data: { data: {} } }));
const toBoolStatus = (isActive) => (isActive ? 'Active' : 'Inactive');

// ── Real API layer — calls ASP.NET Core backend ───────────────
const api = {
  // Auth
  login: async (email, password) => {
    const res = await axiosClient.post('/auth/login', { email, password });
    const d = unwrap(res);
    if (!d?.token) throw new Error(res.data?.message || 'Login failed');
    return { token: d.token, role: d.role, fullName: d.fullName, userId: d.userId, customerId: d.customerId ?? null };
  },

  register: async (form) => {
    const res = await axiosClient.post('/auth/register-customer', {
      fullName: form.fullName, email: form.email, password: form.password,
      phone: form.phone, address: form.address,
      vehicleNumber: form.vehicleNumber, vehicleBrand: form.vehicleBrand,
      vehicleModel: form.vehicleModel, vehicleType: form.vehicleType,
    });
    if (!res.data?.success) throw new Error(res.data?.message || 'Registration failed');
    return res.data;
  },

  // Staff
  getStaff: async () => {
    const res = await safe(axiosClient.get('/staff'));
    const list = unwrap(res) ?? [];
    return list.map(s => ({
      id: s.id,
      fullName: s.fullName ?? '',
      email: s.email ?? '',
      phone: s.phoneNumber ?? '',
      position: s.position ?? '',
      status: toBoolStatus(s.isActive ?? true),
      joinedDate: s.createdAt ?? null,
      isActive: s.isActive ?? true,
    }));
  },
  createStaff: async (data) => { const res = await axiosClient.post('/staff', data); return unwrap(res); },
  updateStaff: async (id, data) => { const res = await axiosClient.put(`/staff/${id}`, data); return unwrap(res); },
  deleteStaff: async (id) => axiosClient.delete(`/staff/${id}`),
  toggleStaffStatus: async (id) => { const res = await axiosClient.patch(`/staff/${id}/status`); return unwrap(res); },

  // Vendors
  getVendors: async () => {
    const res = await safe(axiosClient.get('/vendors'));
    const list = unwrap(res) ?? [];
    return list.map(v => ({
      id: v.id,
      vendorName: v.vendorName ?? '',
      contactPerson: v.contactPerson ?? '',
      phone: v.phone ?? '',
      email: v.email ?? '',
      address: v.address ?? '',
      status: toBoolStatus(v.isActive ?? true),
      isActive: v.isActive ?? true,
    }));
  },
  createVendor: async (data) => { const res = await axiosClient.post('/vendors', data); return unwrap(res); },
  updateVendor: async (id, data) => { const res = await axiosClient.put(`/vendors/${id}`, data); return unwrap(res); },
  deleteVendor: async (id) => axiosClient.delete(`/vendors/${id}`),

  // Parts
  getParts: async () => {
    const res = await safe(axiosClient.get('/parts'));
    const list = unwrap(res) ?? [];
    return list.map(p => ({
      id: p.id,
      partName: p.partName ?? '',
      partCode: p.partCode ?? '',
      category: p.category ?? '',
      description: p.description ?? '',
      unitPrice: p.unitPrice ?? 0,
      stockQty: p.stockQuantity ?? 0,
      stockQuantity: p.stockQuantity ?? 0,
      reorderLevel: p.reorderLevel ?? 10,
      vendorId: p.vendorId ?? '',
      vendorName: p.vendor?.vendorName ?? p.vendorName ?? '',
      status: toBoolStatus(p.isActive ?? true),
      isActive: p.isActive ?? true,
    }));
  },
  createPart: async (data) => { const res = await axiosClient.post('/parts', data); return unwrap(res); },
  updatePart: async (id, data) => { const res = await axiosClient.put(`/parts/${id}`, data); return unwrap(res); },
  deletePart: async (id) => axiosClient.delete(`/parts/${id}`),
  getLowStockParts: async () => {
    const res = await safe(axiosClient.get('/parts/low-stock'));
    const list = unwrap(res) ?? [];
    return list.map(p => ({
      id: p.id,
      partName: p.partName ?? '',
      partCode: p.partCode ?? '',
      category: p.category ?? '',
      unitPrice: p.unitPrice ?? 0,
      stockQty: p.stockQuantity ?? 0,
      stockQuantity: p.stockQuantity ?? 0,
      reorderLevel: p.reorderLevel ?? 10,
      vendorId: p.vendorId ?? '',
      vendorName: p.vendor?.vendorName ?? p.vendorName ?? '',
      status: toBoolStatus(p.isActive ?? true),
      isActive: p.isActive ?? true,
    }));
  },

  // Customers
  getCustomers: async () => {
    const res = await safe(axiosClient.get('/customers'));
    return (unwrap(res) ?? []).map(c => ({
      id: c.id, fullName: c.user?.fullName ?? '', email: c.user?.email ?? '',
      phone: c.user?.phoneNumber ?? '', address: c.address ?? '',
      creditBalance: c.creditBalance ?? 0, vehicles: c.vehicles ?? [],
    }));
  },

  getMyCustomer: async () => {
    const res = await safeObj(axiosClient.get('/customers/me'));
    return unwrap(res) ?? null;
  },

  createCustomer: async (data) => { const res = await axiosClient.post('/customers', data); return unwrap(res); },
  updateCustomer: async (id, data) => { const res = await axiosClient.put(`/customers/${id}`, data); return unwrap(res); },

  searchCustomers: async (keyword) => {
    const res = await safe(axiosClient.get(`/customers/search?keyword=${encodeURIComponent(keyword)}`));
    return (unwrap(res) ?? []).map(c => ({
      id: c.id, fullName: c.user?.fullName ?? '', email: c.user?.email ?? '',
      phone: c.user?.phoneNumber ?? '', address: c.address ?? '',
      creditBalance: c.creditBalance ?? 0, vehicles: c.vehicles ?? [],
    }));
  },

  getVehicles: async (customerId) => {
    if (customerId) {
      const res = await safe(axiosClient.get(`/customers/${customerId}/vehicles`));
      return unwrap(res) ?? [];
    }
    const res = await safe(axiosClient.get('/customers'));
    const customers = unwrap(res) ?? [];
    return customers.flatMap(c => (c.vehicles ?? []).map(v => ({ ...v, customerId: c.id })));
  },

  addVehicle: async (customerId, data) => { const res = await axiosClient.post(`/customers/${customerId}/vehicles`, data); return unwrap(res); },
  updateVehicle: async (customerId, vehicleId, data) => { const res = await axiosClient.put(`/customers/${customerId}/vehicles/${vehicleId}`, data); return unwrap(res); },
  deleteVehicle: async (customerId, vehicleId) => axiosClient.delete(`/customers/${customerId}/vehicles/${vehicleId}`),
  getCustomerHistory: async (customerId) => {
    const res = await safe(axiosClient.get(`/customers/${customerId}/history`));
    const d = unwrap(res) ?? {};
    return { purchases: d.purchases ?? [], appointments: d.appointments ?? [] };
  },
  getCustomerCreditSummary: async (customerId) => { const res = await safe(axiosClient.get(`/customers/${customerId}/credit-summary`)); return unwrap(res) ?? {}; },

  // Sales Invoices
  getSalesInvoices: async () => {
    const res = await safe(axiosClient.get('/salesinvoices'));
    const list = unwrap(res) ?? [];
    return list.map(i => ({
      ...i,
      customerName: i.customer?.user?.fullName ?? i.customerName ?? 'Unknown',
      staffName: i.staff?.fullName ?? i.staffName ?? '',
      date: i.date,
      emailSent: i.emailSent ?? false,
    }));
  },
  createSalesInvoice: async (data) => { const res = await axiosClient.post('/salesinvoices', data); return unwrap(res); },
  sendInvoiceEmail: async (invoiceId) => { const res = await axiosClient.post(`/salesinvoices/${invoiceId}/send-email`); return unwrap(res); },
  getCustomerInvoices: async (customerId) => { const res = await safe(axiosClient.get(`/salesinvoices/customer/${customerId}`)); return unwrap(res) ?? []; },

  // Purchase Invoices
  getPurchaseInvoices: async () => {
    const res = await safe(axiosClient.get('/purchaseinvoices'));
    const list = unwrap(res) ?? [];
    return list.map(i => ({
      ...i,
      vendorName: i.vendor?.vendorName ?? i.vendorName ?? '',
      date: i.date,
    }));
  },
  createPurchaseInvoice: async (data) => { const res = await axiosClient.post('/purchaseinvoices', data); return unwrap(res); },

  // Appointments
  getAppointments: async () => {
    const res = await safe(axiosClient.get('/appointments'));
    const list = unwrap(res) ?? [];
    return list.map(a => ({
      ...a,
      customerName: a.customer?.user?.fullName ?? a.customerName ?? '',
      vehicleNumber: a.vehicle?.vehicleNumber ?? a.vehicleNumber ?? '',
      date: a.appointmentDate ?? a.date,
    }));
  },
  createAppointment: async (data) => { const res = await axiosClient.post('/appointments', data); return unwrap(res); },
  updateAppointmentStatus: async (id, status) => axiosClient.patch(`/appointments/${id}/status`, JSON.stringify(status), { headers: { 'Content-Type': 'application/json' } }),

  // Part Requests
  getPartRequests: async () => {
    const res = await safe(axiosClient.get('/unavailablepartrequests'));
    const list = unwrap(res) ?? [];
    return list.map(r => ({
      ...r,
      customerName: r.customer?.user?.fullName ?? r.customerName ?? '',
      vehicleNumber: r.vehicle?.vehicleNumber ?? r.vehicleNumber ?? '—',
      date: r.requestDate ?? r.date,
    }));
  },
  createPartRequest: async (data) => { const res = await axiosClient.post('/unavailablepartrequests', data); return unwrap(res); },
  updatePartRequestStatus: async (id, status) => axiosClient.patch(`/unavailablepartrequests/${id}/status`, JSON.stringify(status), { headers: { 'Content-Type': 'application/json' } }),

  // Reviews
  getReviews: async () => {
    const res = await safe(axiosClient.get('/reviews'));
    const list = unwrap(res) ?? [];
    return list.map(r => ({
      ...r,
      customerName: r.customer?.user?.fullName ?? r.customerName ?? '',
      date: r.date,
    }));
  },
  getCustomerReviews: async (customerId) => { const res = await safe(axiosClient.get(`/reviews/customer/${customerId}`)); return unwrap(res) ?? []; },
  createReview: async (data) => { const res = await axiosClient.post('/reviews', data); return unwrap(res); },

  // Notifications
  getNotifications: async () => { const res = await safe(axiosClient.get('/notifications')); return unwrap(res) ?? []; },
  markNotificationRead: async (id) => axiosClient.patch(`/notifications/${id}/read`),

  // AI Predictions
  getPredictions: async (customerId) => { const res = await safe(axiosClient.get(`/ai/predictions/customer/${customerId}`)); return unwrap(res) ?? []; },

  // Reports - FIXED: correct URL format
  getFinancialReport: async (period = 'monthly', params = {}) => {
    const url = `/reports/financial/${period}`;
    const res = await safeObj(axiosClient.get(url, { params }));
    return unwrap(res) ?? {};
  },

  getCreditOverdue: async () => { const res = await safe(axiosClient.get('/reports/customers/pending-credits')); return unwrap(res) ?? []; },
  getHighSpenders: async () => { const res = await safe(axiosClient.get('/reports/customers/high-spenders')); return unwrap(res) ?? []; },
  getRegularCustomers: async () => { const res = await safe(axiosClient.get('/reports/customers/regulars')); return unwrap(res) ?? []; },
  sendCreditReminders: async () => { const res = await axiosClient.post('/credits/send-reminders'); return unwrap(res); },
  sendCreditReminder: async (invoiceId) => { const res = await axiosClient.post(`/credits/send-reminder/${invoiceId}`); return unwrap(res); },
  getEmailLogs: async () => { const res = await safe(axiosClient.get('/emaillogs')); return unwrap(res) ?? []; },
  seedEmailLogs: async () => { const res = await axiosClient.post('/emaillogs/seed'); return unwrap(res); },
  // Dev endpoints (no-auth in DEBUG) to help testing UI evidence
  getDevEmailLogs: async () => { const res = await safe(axiosClient.get('/dev/emaillogs')); return unwrap(res) ?? []; },
  seedDevEmailLogs: async () => { const res = await axiosClient.post('/dev/emaillogs/seed'); return unwrap(res); },

  // Admin dashboard stats (computed from multiple endpoints — admin only)
  getAdminStats: async () => {
    const [staff, vendors, parts, lowStock, report] = await Promise.all([
      safe(axiosClient.get('/staff')),
      safe(axiosClient.get('/vendors')),
      safe(axiosClient.get('/parts')),
      safe(axiosClient.get('/parts/low-stock')),
      safeObj(axiosClient.get('/reports/financial/monthly')),
    ]);
    const staffList  = unwrap(staff)   ?? [];
    const vendorList = unwrap(vendors)  ?? [];
    const partsList  = unwrap(parts)    ?? [];
    const lowList    = unwrap(lowStock) ?? [];
    const rep        = unwrap(report)   ?? {};
    return {
      totalStaff:     Array.isArray(staffList)  ? staffList.length  : 0,
      totalVendors:   Array.isArray(vendorList) ? vendorList.length : 0,
      totalParts:     Array.isArray(partsList)  ? partsList.length  : 0,
      lowStockParts:  Array.isArray(lowList)    ? lowList.length    : 0,
      todaySales:     0,
      monthlyRevenue: rep.netRevenue    ?? 0,
      pendingCredit:  rep.creditSales   ?? 0,
      totalSalesToday: 0,
      customersToday: 0,
    };
  },

  // Staff dashboard stats (only accesses endpoints staff can reach)
  getStaffStats: async () => {
    const [invoices, credits] = await Promise.all([
      safe(axiosClient.get('/salesinvoices')),
      safe(axiosClient.get('/reports/customers/pending-credits')),
    ]);
    const invList    = unwrap(invoices) ?? [];
    const creditList = unwrap(credits)  ?? [];
    const today = new Date().toDateString();
    const todayInvoices = invList.filter(i => new Date(i.date).toDateString() === today);
    return {
      totalSalesToday: todayInvoices.length,
      customersToday:  new Set(todayInvoices.map(i => i.customerId)).size,
      todaySales:      todayInvoices.reduce((s, i) => s + (i.totalAmount ?? 0), 0),
      pendingCredit:   creditList.reduce((s, i) => s + (i.totalAmount ?? 0), 0),
    };
  },

  // Backward compat alias — use getAdminStats for admin, getStaffStats for staff
  getStats: async () => {
    const user = JSON.parse(storage.getItem('vp_user') || 'null');
    return user?.role === 'Admin' ? api.getAdminStats() : api.getStaffStats();
  },

  // Payments
  initiateEsewa: async (invoiceId) => { const res = await axiosClient.post(`/payment/esewa/initiate/${invoiceId}`); return unwrap(res); },
  verifyEsewa: async (data, invoiceId) => { const res = await axiosClient.post(`/payment/esewa/verify?data=${data}&invoiceId=${invoiceId}`); return unwrap(res); },
  initiateKhalti: async (invoiceId) => { const res = await axiosClient.post(`/payment/khalti/initiate/${invoiceId}`); return unwrap(res); },
  verifyKhalti: async (pidx, invoiceId) => { const res = await axiosClient.post(`/payment/khalti/verify?pidx=${pidx}&invoiceId=${invoiceId}`); return unwrap(res); }
};

export { auth, navigate, getPath, formatCurrency, formatDate, formatDateTime, DEMO, api };
