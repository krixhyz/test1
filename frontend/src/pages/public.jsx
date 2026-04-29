import { PublicLayout } from '../components/layout.jsx';
import { Icon, Button, Input, Select, Alert, FormRow } from '../components/common.jsx';
import { auth, api, navigate, getPath, formatCurrency, formatDate, formatDateTime, DEMO } from '../utils.js';
// ── Public Pages — Glassmorphism ─────────────────────────────
import React, { useState  } from 'react';

function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPw, setShowPw] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault(); setError('');
    if (!email) { setError('Email is required'); return; }
    if (!password) { setError('Password is required'); return; }
    setLoading(true);
    try {
      const data = await api.login(email, password);
      auth.setAuth({ fullName: data.fullName, role: data.role, userId: data.userId }, data.token);
      const map = { Admin:'/admin/dashboard', Staff:'/staff/dashboard', Customer:'/customer/dashboard' };
      navigate(map[data.role] || '/login');
      window.location.reload();
    } catch (err) {
      setError(err.message || 'Invalid credentials. Please try again.');
    } finally { setLoading(false); }
  };

  return (
    <PublicLayout>
      <h2 className="vp-auth-title">Sign In</h2>
      <p className="vp-auth-subtitle">Enter your credentials to access the system</p>
      {error && <Alert type="error" message={error} onClose={() => setError('')} />}
      <form onSubmit={handleSubmit}>
        <Input label="Email Address" id="email" type="email" value={email}
          onChange={e => setEmail(e.target.value)} placeholder="you@vparts.com" required />
        <div className="vp-form-group">
          <label className="vp-label">Password <span className="vp-required">*</span></label>
          <div className="vp-pw-wrap">
            <input type={showPw ? 'text' : 'password'} className="vp-input"
              value={password} onChange={e => setPassword(e.target.value)} placeholder="••••••••" />
            <button type="button" className="vp-pw-toggle" onClick={() => setShowPw(p => !p)}>
              {showPw ? <Icon name="eye_off" size={16} color="rgba(255,255,255,0.4)" /> : <Icon name="eye" size={16} color="rgba(255,255,255,0.4)" />}
            </button>
          </div>
        </div>
        <Button type="submit" disabled={loading} className="vp-btn-full" size="lg" style={{marginTop:6}}>
          {loading ? 'Signing in...' : 'Sign In'}
        </Button>
      </form>
      <div className="vp-auth-footer">
        <span>New customer? </span>
        <a href="#/register" className="vp-auth-link">Create account</a>
      </div>
      <div className="vp-demo-box">
        <div className="vp-demo-label">Demo Credentials</div>
        <div className="vp-demo-row"><strong style={{color:'rgba(255,255,255,0.7)'}}>Admin:</strong> admin@vparts.com / Admin@123</div>
        <div className="vp-demo-row"><strong style={{color:'rgba(255,255,255,0.7)'}}>Staff:</strong> staff@vparts.com / Staff@123</div>
        <div className="vp-demo-row"><strong style={{color:'rgba(255,255,255,0.7)'}}>Customer:</strong> Register via Create Account</div>
      </div>
    </PublicLayout>
  );
}

function Register() {
  const [form, setForm] = useState({ fullName:'', email:'', phone:'', password:'', confirmPassword:'', address:'', vehicleNumber:'', vehicleBrand:'', vehicleModel:'', vehicleType:'Car' });
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState('');
  const set = k => e => setForm(f => ({ ...f, [k]: e.target.value }));

  const validate = () => {
    const e = {};
    if (!form.fullName) e.fullName = 'Required';
    if (!form.phone) e.phone = 'Required';
    if (!form.email) e.email = 'Required';
    else if (!/\S+@\S+\.\S+/.test(form.email)) e.email = 'Invalid email';
    if (!form.password) e.password = 'Required';
    else if (form.password.length < 6) e.password = 'Min. 6 characters';
    if (form.password !== form.confirmPassword) e.confirmPassword = 'Passwords do not match';
    if (!form.vehicleNumber) e.vehicleNumber = 'Required';
    return e;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const errs = validate();
    if (Object.keys(errs).length) { setErrors(errs); return; }
    setLoading(true);
    try {
      await api.register(form);
      setSuccess('Account created. Redirecting to login...');
      setTimeout(() => navigate('/login'), 1500);
    } catch (err) {
      setErrors({ general: err.message || 'Registration failed.' });
    } finally { setLoading(false); }
  };

  return (
    <PublicLayout>
      <h2 className="vp-auth-title">Create Account</h2>
      <p className="vp-auth-subtitle">Register as a customer</p>
      {errors.general && <Alert type="error" message={errors.general} />}
      {success && <Alert type="success" message={success} />}
      <form onSubmit={handleSubmit}>
        <p className="vp-section-label">Personal Details</p>
        <FormRow>
          <Input label="Full Name" value={form.fullName} onChange={set('fullName')} placeholder="Full name" error={errors.fullName} required />
          <Input label="Phone" value={form.phone} onChange={set('phone')} placeholder="98XXXXXXXX" error={errors.phone} required />
        </FormRow>
        <Input label="Email Address" type="email" value={form.email} onChange={set('email')} placeholder="you@email.com" error={errors.email} required />
        <Input label="Address" value={form.address} onChange={set('address')} placeholder="City, District" />
        <FormRow>
          <div className="vp-form-group">
            <label className="vp-label">Password <span className="vp-required">*</span></label>
            <input type="password" className={`vp-input ${errors.password?'vp-input-error':''}`} value={form.password} onChange={set('password')} placeholder="Min. 6 characters" />
            {errors.password && <p className="vp-field-error">{errors.password}</p>}
          </div>
          <div className="vp-form-group">
            <label className="vp-label">Confirm Password <span className="vp-required">*</span></label>
            <input type="password" className={`vp-input ${errors.confirmPassword?'vp-input-error':''}`} value={form.confirmPassword} onChange={set('confirmPassword')} placeholder="Repeat password" />
            {errors.confirmPassword && <p className="vp-field-error">{errors.confirmPassword}</p>}
          </div>
        </FormRow>
        <p className="vp-section-label" style={{marginTop:16}}>Vehicle Details</p>
        <FormRow>
          <Input label="Vehicle Number" value={form.vehicleNumber} onChange={set('vehicleNumber')} placeholder="BA 1 PA 1234" error={errors.vehicleNumber} required />
          <Select label="Vehicle Type" value={form.vehicleType} onChange={set('vehicleType')}>
            {['Car','SUV','Truck','Van','Motorcycle','Other'].map(t => <option key={t}>{t}</option>)}
          </Select>
        </FormRow>
        <FormRow>
          <Input label="Brand" value={form.vehicleBrand} onChange={set('vehicleBrand')} placeholder="Toyota, Honda..." />
          <Input label="Model" value={form.vehicleModel} onChange={set('vehicleModel')} placeholder="Vitz, City..." />
        </FormRow>
        <Button type="submit" disabled={loading} className="vp-btn-full" size="lg" style={{marginTop:8}}>
          {loading ? 'Creating account...' : 'Create Account'}
        </Button>
      </form>
      <div className="vp-auth-footer">
        <span>Already have an account? </span>
        <a href="#/login" className="vp-auth-link">Sign in</a>
      </div>
    </PublicLayout>
  );
}

function Home() {
  return (
    <div className="vp-home">
      <div className="vp-home-content">
        <div className="vp-home-mark">VP</div>
        <h1 className="vp-home-title">VehicleParts</h1>
        <p className="vp-home-sub">Selling &amp; Inventory Management System</p>
        <div className="vp-home-actions">
          <a href="#/login" className="vp-btn vp-btn-primary vp-btn-lg">Sign In</a>
          <a href="#/register" className="vp-btn vp-btn-secondary vp-btn-lg">Register</a>
        </div>
      </div>
    </div>
  );
}

export { Login, Register, Home  };
