import { Register } from '../pages/public.jsx';
import { Button } from '../components/common.jsx';
import { auth, api, navigate, getPath, formatCurrency, formatDate, formatDateTime, DEMO } from '../utils.js';
// ── Layout — Glassmorphism ────────────────────────────────────
import React, { useState  } from 'react';

function NavIcon({ name, size=16 }) {
  const s = { stroke:'currentColor', strokeWidth:'1.8', fill:'none', strokeLinecap:'square' };
  const icons = {
    dashboard: <svg width={size} height={size} viewBox="0 0 24 24" {...s}><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/></svg>,
    staff:     <svg width={size} height={size} viewBox="0 0 24 24" {...s}><path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 00-3-3.87"/><path d="M16 3.13a4 4 0 010 7.75"/></svg>,
    vendor:    <svg width={size} height={size} viewBox="0 0 24 24" {...s}><path d="M3 9l9-7 9 7v11a2 2 0 01-2 2H5a2 2 0 01-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>,
    parts:     <svg width={size} height={size} viewBox="0 0 24 24" {...s}><circle cx="12" cy="12" r="3"/><path d="M19.07 4.93l-1.41 1.41M5.34 18.66l-1.41 1.41M20 12h-2M6 12H4M19.07 19.07l-1.41-1.41M5.34 5.34L3.93 3.93M12 20v-2M12 6V4"/></svg>,
    purchase:  <svg width={size} height={size} viewBox="0 0 24 24" {...s}><path d="M21 16V8a2 2 0 00-1-1.73l-7-4a2 2 0 00-2 0l-7 4A2 2 0 003 8v8a2 2 0 001 1.73l7 4a2 2 0 002 0l7-4A2 2 0 0021 16z"/></svg>,
    reports:   <svg width={size} height={size} viewBox="0 0 24 24" {...s}><line x1="18" y1="20" x2="18" y2="10"/><line x1="12" y1="20" x2="12" y2="4"/><line x1="6" y1="20" x2="6" y2="14"/></svg>,
    lowstock:  <svg width={size} height={size} viewBox="0 0 24 24" {...s}><path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>,
    bell:      <svg width={size} height={size} viewBox="0 0 24 24" {...s}><path d="M18 8A6 6 0 006 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 01-3.46 0"/></svg>,
    add_user:  <svg width={size} height={size} viewBox="0 0 24 24" {...s}><path d="M16 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2"/><circle cx="8.5" cy="7" r="4"/><line x1="20" y1="8" x2="20" y2="14"/><line x1="23" y1="11" x2="17" y2="11"/></svg>,
    search:    <svg width={size} height={size} viewBox="0 0 24 24" {...s}><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>,
    sale:      <svg width={size} height={size} viewBox="0 0 24 24" {...s}><circle cx="9" cy="21" r="1"/><circle cx="20" cy="21" r="1"/><path d="M1 1h4l2.68 13.39a2 2 0 002 1.61h9.72a2 2 0 002-1.61L23 6H6"/></svg>,
    invoice:   <svg width={size} height={size} viewBox="0 0 24 24" {...s}><path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/></svg>,
    credit:    <svg width={size} height={size} viewBox="0 0 24 24" {...s}><rect x="1" y="4" width="22" height="16"/><line x1="1" y1="10" x2="23" y2="10"/></svg>,
    profile:   <svg width={size} height={size} viewBox="0 0 24 24" {...s}><path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>,
    car:       <svg width={size} height={size} viewBox="0 0 24 24" {...s}><path d="M5 17H3v-5l3-6h12l3 6v5h-2m-7 0H7m10 0a2 2 0 11-4 0 2 2 0 014 0zm-10 0a2 2 0 11-4 0 2 2 0 014 0z"/></svg>,
    calendar:  <svg width={size} height={size} viewBox="0 0 24 24" {...s}><rect x="3" y="4" width="18" height="18"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>,
    tool:      <svg width={size} height={size} viewBox="0 0 24 24" {...s}><path d="M14.7 6.3a1 1 0 000 1.4l1.6 1.6a1 1 0 001.4 0l3.77-3.77a6 6 0 01-7.94 7.94l-6.91 6.91a2.12 2.12 0 01-3-3l6.91-6.91a6 6 0 017.94-7.94l-3.76 3.76z"/></svg>,
    history:   <svg width={size} height={size} viewBox="0 0 24 24" {...s}><polyline points="1 4 1 10 7 10"/><path d="M3.51 15a9 9 0 102.13-9.36L1 10"/></svg>,
    star:      <svg width={size} height={size} viewBox="0 0 24 24" {...s}><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>,
    ai:        <svg width={size} height={size} viewBox="0 0 24 24" {...s}><rect x="2" y="3" width="20" height="14"/><line x1="8" y1="21" x2="16" y2="21"/><line x1="12" y1="17" x2="12" y2="21"/></svg>,
    logout:    <svg width={size} height={size} viewBox="0 0 24 24" {...s}><path d="M9 21H5a2 2 0 01-2-2V5a2 2 0 012-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>,
    menu:      <svg width={size} height={size} viewBox="0 0 24 24" {...s}><line x1="3" y1="12" x2="21" y2="12"/><line x1="3" y1="6" x2="21" y2="6"/><line x1="3" y1="18" x2="21" y2="18"/></svg>,
    chevL:     <svg width={size} height={size} viewBox="0 0 24 24" {...s}><polyline points="11 17 6 12 11 7"/><polyline points="18 17 13 12 18 7"/></svg>,
    chevR:     <svg width={size} height={size} viewBox="0 0 24 24" {...s}><polyline points="13 17 18 12 13 7"/><polyline points="6 17 11 12 6 7"/></svg>,
  };
  return icons[name] ? <span style={{display:'flex',alignItems:'center'}}>{icons[name]}</span> : null;
}

const adminLinks = [
  { path:'/admin/dashboard',         label:'Dashboard',        icon:'dashboard' },
  { path:'/admin/staff',             label:'Staff',            icon:'staff'     },
  { path:'/admin/vendors',           label:'Vendors',          icon:'vendor'    },
  { path:'/admin/parts',             label:'Parts',            icon:'parts'     },
  { path:'/admin/purchase-invoices', label:'Purchases',        icon:'purchase'  },
  { path:'/admin/financial-reports', label:'Financial Reports',icon:'reports'   },
  { path:'/admin/low-stock',         label:'Low Stock',        icon:'lowstock'  },
  { path:'/admin/notifications',     label:'Notifications',    icon:'bell'      },
];
const staffLinks = [
  { path:'/staff/dashboard',          label:'Dashboard',        icon:'dashboard' },
  { path:'/staff/customers/register', label:'Register Customer',icon:'add_user'  },
  { path:'/staff/customers/search',   label:'Search Customer',  icon:'search'    },
  { path:'/staff/sales',              label:'New Sale',         icon:'sale'      },
  { path:'/staff/invoices',           label:'Invoices',         icon:'invoice'   },
  { path:'/staff/customer-reports',   label:'Customer Reports', icon:'reports'   },
  { path:'/staff/credit-reminders',   label:'Credit Reminders', icon:'credit'    },
];
const customerLinks = [
  { path:'/customer/dashboard',    label:'Dashboard',    icon:'dashboard' },
  { path:'/customer/profile',      label:'My Profile',   icon:'profile'   },
  { path:'/customer/vehicles',     label:'My Vehicles',  icon:'car'       },
  { path:'/customer/appointments', label:'Appointments', icon:'calendar'  },
  { path:'/customer/request-part', label:'Request Part', icon:'tool'      },
  { path:'/customer/history',      label:'History',      icon:'history'   },
  { path:'/customer/reviews',      label:'Reviews',      icon:'star'      },
  { path:'/customer/predictions',  label:'Predictions',  icon:'ai'        },
];

function Sidebar({ links, collapsed, onToggle }) {
  const path = getPath();
  const user = auth.getUser();
  return (
    <aside className={`vp-sidebar ${collapsed ? 'vp-sidebar-collapsed' : ''}`}>
      <div className="vp-sidebar-header">
        {!collapsed
          ? <div className="vp-brand"><div className="vp-brand-mark">VP</div><span className="vp-brand-name">VehicleParts</span></div>
          : <div className="vp-brand-mark vp-brand-mark-solo">VP</div>
        }
        <button className="vp-sidebar-toggle" onClick={onToggle}>
          {collapsed ? <NavIcon name="chevR" size={15} /> : <NavIcon name="chevL" size={15} />}
        </button>
      </div>

      {!collapsed && user && (
        <div className="vp-sidebar-user">
          <div className="vp-user-avatar">{(user.fullName||'U')[0].toUpperCase()}</div>
          <div className="vp-user-info">
            <div className="vp-user-name">{user.fullName}</div>
            <div className="vp-user-role">{user.role}</div>
          </div>
        </div>
      )}

      <nav className="vp-sidebar-nav">
        {links.map(link => (
          <a key={link.path} href={`#${link.path}`}
            className={`vp-nav-link ${path === link.path ? 'vp-nav-active' : ''}`}
            title={collapsed ? link.label : ''}>
            <span className="vp-nav-icon"><NavIcon name={link.icon} size={16} /></span>
            {!collapsed && <span className="vp-nav-label">{link.label}</span>}
          </a>
        ))}
      </nav>

      <div className="vp-sidebar-footer">
        <button className="vp-nav-link vp-logout" title={collapsed ? 'Logout' : ''}
          onClick={() => { auth.clearAuth(); navigate('/login'); window.location.reload(); }}>
          <span className="vp-nav-icon"><NavIcon name="logout" size={16} /></span>
          {!collapsed && <span className="vp-nav-label">Logout</span>}
        </button>
      </div>
    </aside>
  );
}

function Topbar({ title, onMenuClick }) {
  const user = auth.getUser();
  return (
    <header className="vp-topbar">
      <div className="vp-topbar-left">
        <button className="vp-menu-btn" onClick={onMenuClick}><NavIcon name="menu" size={20} /></button>
        <span className="vp-topbar-title">{title}</span>
      </div>
      <div className="vp-topbar-right">
        {user && <div className="vp-topbar-user">
          <div className="vp-topbar-avatar">{(user.fullName||'U')[0].toUpperCase()}</div>
          <span className="vp-topbar-name">{user.fullName}</span>
        </div>}
      </div>
    </header>
  );
}

function DashboardLayout({ links, title, children }) {
  const [collapsed, setCollapsed] = useState(false);
  return (
    <div className={`vp-layout ${collapsed ? 'vp-layout-collapsed' : ''}`}>
      <Sidebar links={links} collapsed={collapsed} onToggle={() => setCollapsed(c => !c)} />
      <div className="vp-main-area">
        <Topbar title={title} onMenuClick={() => setCollapsed(c => !c)} />
        <main className="vp-main-content">{children}</main>
      </div>
    </div>
  );
}

function AdminLayout({ children, title='Admin Panel' })       { return <DashboardLayout links={adminLinks}    title={title}>{children}</DashboardLayout>; }
function StaffLayout({ children, title='Staff Panel' })       { return <DashboardLayout links={staffLinks}    title={title}>{children}</DashboardLayout>; }
function CustomerLayout({ children, title='My Account' })     { return <DashboardLayout links={customerLinks} title={title}>{children}</DashboardLayout>; }

function PublicLayout({ children }) {
  return (
    <div className="vp-public">
      <div className="vp-public-brand">
        <div className="vp-public-mark">VP</div>
        <div className="vp-public-name">VehicleParts</div>
        <div className="vp-public-tagline">Selling &amp; Inventory Management System</div>
      </div>
      <div className="vp-public-card">{children}</div>
    </div>
  );
}

function ProtectedRoute({ children, allowedRoles }) {
  const user = auth.getUser();
  if (!auth.getToken() || !user) { navigate('/login'); return null; }
  if (allowedRoles && !allowedRoles.includes(user.role)) {
    return (
      <div className="vp-unauthorized">
        <div className="vp-eyebrow">Access Denied</div>
        <h2 className="vp-page-title" style={{marginTop:8}}>403 — Forbidden</h2>
        <p className="vp-body-muted" style={{marginTop:6}}>You do not have permission to view this page.</p>
        <Button onClick={() => navigate(`/${user.role.toLowerCase()}/dashboard`)} style={{marginTop:20}}>Go to Dashboard</Button>
      </div>
    );
  }
  return children;
}

export { AdminLayout, StaffLayout, CustomerLayout, PublicLayout, ProtectedRoute, NavIcon  };
