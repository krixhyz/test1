import { CustomerDashboard, CustomerProfile, MyVehicles, BookAppointment, RequestUnavailablePart, MyHistory, SubmitReview, PartFailurePrediction } from './pages/customer.jsx';
import { StaffDashboard, CustomerRegistration, CustomersPage, CustomerSearch, PartsSale, SalesInvoices, CustomerDetails, CustomerReports, CreditReminders } from './pages/staff.jsx';
import { AdminDashboard, StaffManagement, VendorManagement, PartsManagement, PurchaseInvoices, FinancialReports, LowStockAlerts, AdminNotifications } from './pages/admin.jsx';
import { Login, Register, Home } from './pages/public.jsx';
import { ProtectedRoute } from './components/layout.jsx';
import { auth, api, navigate, getPath, formatCurrency, formatDate, formatDateTime, DEMO } from './utils.js';
// ── App Router ────────────────────────────────────────────────
import React, { useState, useEffect  } from 'react';

const routes = {
  '/':                             () => <Home />,
  '/login':                        () => <Login />,
  '/register':                     () => <Register />,

  '/admin/dashboard':              () => <ProtectedRoute allowedRoles={['Admin']}><AdminDashboard /></ProtectedRoute>,
  '/admin/staff':                  () => <ProtectedRoute allowedRoles={['Admin']}><StaffManagement /></ProtectedRoute>,
  '/admin/vendors':                () => <ProtectedRoute allowedRoles={['Admin']}><VendorManagement /></ProtectedRoute>,
  '/admin/parts':                  () => <ProtectedRoute allowedRoles={['Admin']}><PartsManagement /></ProtectedRoute>,
  '/admin/purchase-invoices':      () => <ProtectedRoute allowedRoles={['Admin']}><PurchaseInvoices /></ProtectedRoute>,
  '/admin/financial-reports':      () => <ProtectedRoute allowedRoles={['Admin']}><FinancialReports /></ProtectedRoute>,
  '/admin/low-stock':              () => <ProtectedRoute allowedRoles={['Admin']}><LowStockAlerts /></ProtectedRoute>,
  '/admin/notifications':          () => <ProtectedRoute allowedRoles={['Admin']}><AdminNotifications /></ProtectedRoute>,

  '/staff/dashboard':              () => <ProtectedRoute allowedRoles={['Staff']}><StaffDashboard /></ProtectedRoute>,
  '/staff/customers':              () => <ProtectedRoute allowedRoles={['Staff']}><CustomersPage /></ProtectedRoute>,
  '/staff/customers/register':     () => <ProtectedRoute allowedRoles={['Staff']}><CustomerRegistration /></ProtectedRoute>,
  '/staff/customers/search':       () => <ProtectedRoute allowedRoles={['Staff']}><CustomerSearch /></ProtectedRoute>,
  '/staff/sales':                  () => <ProtectedRoute allowedRoles={['Staff']}><PartsSale /></ProtectedRoute>,
  '/staff/invoices':               () => <ProtectedRoute allowedRoles={['Staff']}><SalesInvoices /></ProtectedRoute>,
  '/staff/customer-reports':       () => <ProtectedRoute allowedRoles={['Staff']}><CustomerReports /></ProtectedRoute>,
  '/staff/credit-reminders':       () => <ProtectedRoute allowedRoles={['Staff']}><CreditReminders /></ProtectedRoute>,

  '/customer/dashboard':           () => <ProtectedRoute allowedRoles={['Customer']}><CustomerDashboard /></ProtectedRoute>,
  '/customer/profile':             () => <ProtectedRoute allowedRoles={['Customer']}><CustomerProfile /></ProtectedRoute>,
  '/customer/vehicles':            () => <ProtectedRoute allowedRoles={['Customer']}><MyVehicles /></ProtectedRoute>,
  '/customer/appointments':        () => <ProtectedRoute allowedRoles={['Customer']}><BookAppointment /></ProtectedRoute>,
  '/customer/request-part':        () => <ProtectedRoute allowedRoles={['Customer']}><RequestUnavailablePart /></ProtectedRoute>,
  '/customer/history':             () => <ProtectedRoute allowedRoles={['Customer']}><MyHistory /></ProtectedRoute>,
  '/customer/reviews':             () => <ProtectedRoute allowedRoles={['Customer']}><SubmitReview /></ProtectedRoute>,
  '/customer/predictions':         () => <ProtectedRoute allowedRoles={['Customer']}><PartFailurePrediction /></ProtectedRoute>,
};

// Dynamic customer detail route matcher
function matchRoute(path) {
  if (routes[path]) return routes[path];
  if (/^\/staff\/customers\/[^/]+$/.test(path)) return () => <ProtectedRoute allowedRoles={['Staff']}><CustomerDetails /></ProtectedRoute>;
  return null;
}

function App() {
  const [path, setPath] = useState(getPath());

  useEffect(() => {
    const onHash = () => setPath(getPath());
    window.addEventListener('hashchange', onHash);
    return () => window.removeEventListener('hashchange', onHash);
  }, []);

  // Auto-redirect logged-in users from / or /login
  useEffect(() => {
    const user = auth.getUser();
    if (user && (path === '/' || path === '/login')) {
      const map = { Admin: '/admin/dashboard', Staff: '/staff/dashboard', Customer: '/customer/dashboard' };
      navigate(map[user.role] || '/login');
    }
    // Redirect unauthenticated from protected paths
    if (!user && path !== '/login' && path !== '/register' && path !== '/') {
      navigate('/login');
    }
  }, [path]);

  const render = matchRoute(path);
  if (!render) {
    return (
      <div className="not-found">
        <h2>404 — Page Not Found</h2>
        <p>The page you are looking for does not exist.</p>
        <a href="#/login" className="btn btn-primary">Go to Login</a>
      </div>
    );
  }

  return render();
}

export default App;
