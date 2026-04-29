// ── Common UI Components — Glassmorphism ─────────────────────
import React, { useState, useEffect  } from 'react';

function Icon({ name, size = 16, color = 'currentColor' }) {
  const p = { strokeLinecap: 'square', strokeLinejoin: 'miter' };
  const icons = {
    search:   <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="2" {...p}><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>,
    x:        <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="2" {...p}><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>,
    eye:      <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="2" {...p}><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>,
    eye_off:  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="2" {...p}><path d="M17.94 17.94A10.07 10.07 0 0112 20c-7 0-11-8-11-8a18.45 18.45 0 015.06-5.94M9.9 4.24A9.12 9.12 0 0112 4c7 0 11 8 11 8a18.5 18.5 0 01-2.16 3.19m-6.72-1.07a3 3 0 11-4.24-4.24"/><line x1="1" y1="1" x2="23" y2="23"/></svg>,
    plus:     <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="2" {...p}><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>,
    chevdown: <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="2" {...p}><polyline points="6 9 12 15 18 9"/></svg>,
    star_f:   <svg width={size} height={size} viewBox="0 0 24 24" fill={color} stroke={color} strokeWidth="1"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>,
    star_e:   <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="1.5"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>,
    print:    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="2" {...p}><polyline points="6 9 6 2 18 2 18 9"/><path d="M6 18H4a2 2 0 01-2-2v-5a2 2 0 012-2h16a2 2 0 012 2v5a2 2 0 01-2 2h-2"/><rect x="6" y="14" width="12" height="8"/></svg>,
    mail:     <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="2" {...p}><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>,
    alert:    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="2" {...p}><path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>,
    info:     <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="2" {...p}><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>,
    check:    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="2.5" {...p}><polyline points="20 6 9 17 4 12"/></svg>,
  };
  return icons[name] || null;
}

function Button({ children, variant = 'primary', size = 'md', onClick, type = 'button', disabled, className = '', style }) {
  const v = { primary:'vp-btn-primary', secondary:'vp-btn-secondary', danger:'vp-btn-danger', ghost:'vp-btn-ghost', tertiary:'vp-btn-tertiary', success:'vp-btn-success', warning:'vp-btn-warning' };
  const s = { sm:'vp-btn-sm', md:'', lg:'vp-btn-lg' };
  return <button type={type} style={style} className={`vp-btn ${v[variant]||''} ${s[size]||''} ${className}`} onClick={onClick} disabled={disabled}>{children}</button>;
}

function Input({ label, id, error, required, className='', ...props }) {
  return (
    <div className="vp-form-group">
      {label && <label htmlFor={id} className="vp-label">{label}{required && <span className="vp-required"> *</span>}</label>}
      <input id={id} className={`vp-input ${error ? 'vp-input-error' : ''} ${className}`} {...props} />
      {error && <p className="vp-field-error">{error}</p>}
    </div>
  );
}

function Select({ label, id, error, required, children, className='', ...props }) {
  return (
    <div className="vp-form-group">
      {label && <label htmlFor={id} className="vp-label">{label}{required && <span className="vp-required"> *</span>}</label>}
      <div className="vp-select-wrap">
        <select id={id} className={`vp-input vp-select ${error?'vp-input-error':''} ${className}`} {...props}>{children}</select>
        <span className="vp-select-chevron"><Icon name="chevdown" size={14} /></span>
      </div>
      {error && <p className="vp-field-error">{error}</p>}
    </div>
  );
}

function Textarea({ label, id, error, required, ...props }) {
  return (
    <div className="vp-form-group">
      {label && <label htmlFor={id} className="vp-label">{label}{required && <span className="vp-required"> *</span>}</label>}
      <textarea id={id} className={`vp-input ${error?'vp-input-error':''}`} rows={3} {...props} />
      {error && <p className="vp-field-error">{error}</p>}
    </div>
  );
}

function Badge({ children, color='grey' }) {
  const c = { green:'vp-badge-green', red:'vp-badge-red', yellow:'vp-badge-yellow', blue:'vp-badge-blue', orange:'vp-badge-orange', grey:'vp-badge-grey', purple:'vp-badge-purple' };
  return <span className={`vp-badge ${c[color]||'vp-badge-grey'}`}>{children}</span>;
}

function StatusBadge({ status }) {
  const map = { Active:'green', Inactive:'grey', 'In Stock':'green', 'Low Stock':'yellow', 'Out of Stock':'red', Paid:'green', Credit:'orange', Partial:'yellow', Pending:'yellow', Approved:'blue', Completed:'green', Cancelled:'red', Reviewed:'blue', Ordered:'purple', Rejected:'red', High:'red', Medium:'yellow', Low:'green' };
  return <Badge color={map[status]||'grey'}>{status}</Badge>;
}

function stockStatus(part) {
  if (part.stockQty === 0) return 'Out of Stock';
  if (part.stockQty <= part.reorderLevel) return 'Low Stock';
  return 'In Stock';
}

function Card({ children, className='', style }) {
  return <div className={`vp-card ${className}`} style={style}>{children}</div>;
}

function DashboardCard({ title, value, sub, accent }) {
  return (
    <div className="vp-dash-card" style={{ borderTopColor: accent||'var(--accent)' }}>
      <div className="vp-dash-value">{value}</div>
      <div className="vp-dash-title">{title}</div>
      {sub && <div className="vp-dash-sub">{sub}</div>}
    </div>
  );
}

function Loader({ text='Loading...' }) {
  return <div className="vp-loader"><div className="vp-spinner"></div><span className="vp-loader-text">{text}</span></div>;
}

function EmptyState({ message='No records found.' }) {
  return (
    <div className="vp-empty">
      <div className="vp-empty-headline">No records found</div>
      <p className="vp-empty-body">{message}</p>
    </div>
  );
}

function PageHeader({ title, subtitle, action }) {
  return (
    <div className="vp-page-header">
      <div><h1 className="vp-page-title">{title}</h1>{subtitle && <p className="vp-page-subtitle">{subtitle}</p>}</div>
      {action && <div>{action}</div>}
    </div>
  );
}

function SearchBar({ value, onChange, placeholder='Search...' }) {
  return (
    <div className="vp-search">
      <span className="vp-search-icon"><Icon name="search" size={15} /></span>
      <input type="text" className="vp-search-input" value={value} onChange={e => onChange(e.target.value)} placeholder={placeholder} />
    </div>
  );
}

function Alert({ type='error', message, onClose }) {
  if (!message) return null;
  const c = { error:'vp-alert-error', success:'vp-alert-success', warning:'vp-alert-warning', info:'vp-alert-info' };
  return (
    <div className={`vp-alert ${c[type]}`}>
      <span>{message}</span>
      {onClose && <button className="vp-alert-close" onClick={onClose}><Icon name="x" size={15} /></button>}
    </div>
  );
}

function Modal({ open, onClose, title, children, footer, size='md' }) {
  useEffect(() => { document.body.style.overflow = open ? 'hidden' : ''; return () => { document.body.style.overflow = ''; }; }, [open]);
  if (!open) return null;
  const sizes = { sm:'440px', md:'600px', lg:'800px', xl:'960px' };
  return (
    <div className="vp-overlay" onClick={e => { if (e.target === e.currentTarget) onClose(); }}>
      <div className="vp-modal" style={{ maxWidth: sizes[size]||sizes.md }}>
        <div className="vp-modal-header">
          <h3 className="vp-modal-title">{title}</h3>
          <button className="vp-modal-close" onClick={onClose}><Icon name="x" size={18} /></button>
        </div>
        <div className="vp-modal-body">{children}</div>
        {footer && <div className="vp-modal-footer">{footer}</div>}
      </div>
    </div>
  );
}

function ConfirmDialog({ open, onClose, onConfirm, title='Confirm', message }) {
  return (
    <Modal open={open} onClose={onClose} title={title} size="sm"
      footer={<div style={{display:'flex',gap:8,justifyContent:'flex-end'}}><Button variant="ghost" onClick={onClose}>Cancel</Button><Button variant="danger" onClick={onConfirm}>Confirm</Button></div>}>
      <p className="vp-body">{message}</p>
    </Modal>
  );
}

function Table({ columns, data, emptyMessage='No records found.' }) {
  if (!data || data.length === 0) return <EmptyState message={emptyMessage} />;
  return (
    <div className="vp-table-wrap">
      <table className="vp-table">
        <thead><tr>{columns.map(c => <th key={c.key||c.label}>{c.label}</th>)}</tr></thead>
        <tbody>
          {data.map((row, i) => (
            <tr key={row.id||i}>
              {columns.map(c => <td key={c.key||c.label}>{c.render ? c.render(row, i) : (c.key ? row[c.key] : null)}</td>)}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function FormRow({ children }) { return <div className="vp-form-row">{children}</div>; }

function StarRating({ value, onChange }) {
  return (
    <div className="vp-stars">
      {[1,2,3,4,5].map(n => (
        <button key={n} type="button" className="vp-star" onClick={() => onChange && onChange(n)}>
          {n <= value ? <Icon name="star_f" size={22} color="#f59e0b" /> : <Icon name="star_e" size={22} color="#d1d5db" />}
        </button>
      ))}
    </div>
  );
}

function useToast() {
  const [toast, setToast] = useState(null);
  const show = (message, type='success') => { setToast({ message, type }); setTimeout(() => setToast(null), 3200); };
  const Toast = toast ? <div className={`vp-toast vp-toast-${toast.type}`}>{toast.message}</div> : null;
  return { show, Toast };
}

function Eyebrow({ children }) { return <div className="vp-eyebrow">{children}</div>; }

export { Icon, Button, Input, Select, Textarea, Badge, StatusBadge, stockStatus, Card, DashboardCard, Loader, EmptyState, PageHeader, SearchBar, Alert, Modal, ConfirmDialog, Table, FormRow, StarRating, useToast, Eyebrow  };
