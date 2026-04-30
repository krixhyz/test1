import { AdminLayout } from '../components/layout.jsx';
import { Button, Input, Select, Textarea, Badge, StatusBadge, stockStatus, Card, DashboardCard, Loader, EmptyState, PageHeader, SearchBar, Alert, Modal, ConfirmDialog, Table, FormRow, useToast } from '../components/common.jsx';
import { auth, api, navigate, getPath, formatCurrency, formatDate, formatDateTime, DEMO } from '../utils.js';
// ── Admin Pages — Glassmorphism, no emojis ────────────────────
import React, { useState, useEffect  } from 'react';

function AdminDashboard() {
  const [stats, setStats] = useState(null);
  const [notifications, setNotifications] = useState([]);
  const [recentPurchases, setRecentPurchases] = useState([]);
  const [loading, setLoading] = useState(true);
  useEffect(() => {
    Promise.all([api.getAdminStats(), api.getNotifications(), api.getPurchaseInvoices()])
      .then(([s,n,p]) => { setStats(s); setNotifications((n||[]).slice(0,4)); setRecentPurchases((p||[]).slice(0,3)); })
      .catch(()=>{})
      .finally(() => setLoading(false));
  }, []);
  if (loading) return <AdminLayout title="Dashboard"><Loader /></AdminLayout>;
  return (
    <AdminLayout title="Dashboard">
      <PageHeader title="Dashboard" subtitle="System overview" />
      <div className="vp-dash-grid">
        <DashboardCard title="Total Staff"       value={stats.totalStaff}                           accent="var(--accent)" />
        <DashboardCard title="Total Vendors"     value={stats.totalVendors}                         accent="#6366f1" />
        <DashboardCard title="Total Parts"       value={stats.totalParts}                           accent="#06b6d4" />
        <DashboardCard title="Low Stock Parts"   value={stats.lowStockParts}   sub="Needs attention" accent="var(--error)" />
        <DashboardCard title="Today's Sales"     value={formatCurrency(stats.todaySales)}    accent="var(--success)" />
        <DashboardCard title="Monthly Revenue"   value={formatCurrency(stats.monthlyRevenue)} accent="#f59e0b" />
        <DashboardCard title="Pending Credit"    value={formatCurrency(stats.pendingCredit)} accent="#f97316" />
      </div>
      <div className="vp-dash-row">
        <Card className="vp-flex2">
          <div className="vp-section-title">Recent Notifications</div>
          <div className="vp-notif-list">
            {notifications.map(n => (
              <div key={n.id} className={`vp-notif-item ${!n.read?'vp-unread':''}`}>
                <div className="vp-notif-dot"></div>
                <div>
                  <div className="vp-notif-title">{n.title}</div>
                  <div className="vp-notif-msg">{n.message}</div>
                  <div className="vp-notif-time">{formatDateTime(n.createdAt || n.date)}</div>
                </div>
              </div>
            ))}
          </div>
        </Card>
        <Card className="vp-flex1">
          <div className="vp-section-title">Recent Purchases</div>
          {recentPurchases.map(p => (
            <div key={p.id} className="vp-recent-item">
              <div><div className="vp-recent-label">{p.invoiceNumber}</div><div className="vp-recent-sub">{p.vendorName}</div></div>
              <div className="vp-recent-value">{formatCurrency(p.totalAmount)}</div>
            </div>
          ))}
        </Card>
      </div>
    </AdminLayout>
  );
}

function StaffManagement() {
  const [staff, setStaff] = useState([]); const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState(''); const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState(null); const [confirm, setConfirm] = useState(null);
  const { show, Toast } = useToast();
  const empty = { fullName:'', email:'', phone:'', position:'', role:'Staff', status:'Active' };
  const [form, setForm] = useState(empty); const [errors, setErrors] = useState({});
  useEffect(() => { api.getStaff().then(setStaff).finally(() => setLoading(false)); }, []);
  const filtered = staff.filter(s => [s.fullName,s.email,s.phone,s.position].some(v => v?.toLowerCase().includes(search.toLowerCase())));
  const set = k => e => setForm(f => ({ ...f, [k]: e.target.value }));
  const validate = () => { const e={}; if(!form.fullName) e.fullName='Required'; if(!form.email) e.email='Required'; if(!form.phone) e.phone='Required'; if(!form.position) e.position='Required'; return e; };
  const handleSave = async () => {
    const e = validate(); if(Object.keys(e).length){setErrors(e);return;}
    try {
      if(editing) {
        await api.updateStaff(editing.id, {
          fullName: form.fullName,
          phone: form.phone,
          position: form.position || '',
          isActive: form.status === 'Active',
        });
        show('Staff updated');
      } else {
        await api.createStaff({
          fullName: form.fullName,
          email: form.email,
          phone: form.phone,
          password: 'Staff@123',
          position: form.position || '',
          isActive: form.status === 'Active',
        });
        show('Staff added (default password: Staff@123)');
      }
      const fresh = await api.getStaff();
      setStaff(fresh);
      setModal(false);
      setEditing(null);
      setForm(empty);
      setErrors({});
    } catch {
      show('Failed to save staff','error');
    }
  };
  const cols = [
    {label:'Name',key:'fullName'},
    {label:'Email',key:'email'},
    {label:'Phone',key:'phone'},
    {label:'Position',key:'position'},
    {label:'Status',render:s=><StatusBadge status={s.status}/>},
    {label:'Joined',render:s=>formatDate(s.joinedDate)},
    {label:'Actions',render:s=>(
      <div className="vp-action-btns">
        <Button size="sm" variant="ghost" onClick={()=>{setEditing(s);setForm({...s});setErrors({});setModal(true);}}>Edit</Button>
        <Button size="sm" variant={s.status==='Active'?'warning':'success'} onClick={()=>setConfirm(s)}>{s.status==='Active'?'Deactivate':'Activate'}</Button>
      </div>
    )},
  ];
  return (
    <AdminLayout title="Staff Management">
      {Toast}
      <PageHeader title="Staff Management" subtitle={`${staff.length} members`} action={<Button onClick={()=>{setEditing(null);setForm(empty);setErrors({});setModal(true);}}>Add Staff</Button>} />
      <Card>
        <SearchBar value={search} onChange={setSearch} placeholder="Search by name, email, phone..." />
        {loading ? <Loader /> : <Table columns={cols} data={filtered} emptyMessage="No staff found." />}
      </Card>
      <Modal open={modal} onClose={()=>setModal(false)} title={editing?'Edit Staff':'Add Staff'}
        footer={<div style={{display:'flex',gap:8,justifyContent:'flex-end'}}><Button variant="ghost" onClick={()=>setModal(false)}>Cancel</Button><Button onClick={handleSave}>Save</Button></div>}>
        <FormRow><Input label="Full Name" value={form.fullName} onChange={set('fullName')} error={errors.fullName} required placeholder="Full name"/><Input label="Phone" value={form.phone} onChange={set('phone')} error={errors.phone} required placeholder="98XXXXXXXX"/></FormRow>
        <Input label="Email" type="email" value={form.email} onChange={set('email')} error={errors.email} required placeholder="staff@vparts.com"/>
        <FormRow><Input label="Position" value={form.position} onChange={set('position')} error={errors.position} required placeholder="e.g. Cashier"/><Select label="Status" value={form.status} onChange={set('status')}><option>Active</option><option>Inactive</option></Select></FormRow>
      </Modal>
      <ConfirmDialog open={!!confirm} onClose={()=>setConfirm(null)} title="Change Status"
        message={`${confirm?.status==='Active'?'Deactivate':'Activate'} ${confirm?.fullName}?`}
        onConfirm={async()=>{
          if (!confirm) return;
          try {
            await api.toggleStaffStatus(confirm.id);
            const fresh = await api.getStaff();
            setStaff(fresh);
            show('Status updated');
          } catch {
            show('Failed to update status','error');
          }
          setConfirm(null);
        }}/>
    </AdminLayout>
  );
}

function VendorManagement() {
  const [vendors,setVendors]=useState([]); const [loading,setLoading]=useState(true);
  const [search,setSearch]=useState(''); const [modal,setModal]=useState(false);
  const [editing,setEditing]=useState(null); const [confirm,setConfirm]=useState(null);
  const {show,Toast}=useToast();
  const empty={vendorName:'',contactPerson:'',phone:'',email:'',address:'',status:'Active'};
  const [form,setForm]=useState(empty); const [errors,setErrors]=useState({});
  useEffect(()=>{api.getVendors().then(setVendors).finally(()=>setLoading(false));}, []);
  const filtered=vendors.filter(v=>[v.vendorName,v.contactPerson,v.phone].some(x=>x?.toLowerCase().includes(search.toLowerCase())));
  const set=k=>e=>setForm(f=>({...f,[k]:e.target.value}));
  const validate=()=>{const e={};if(!form.vendorName)e.vendorName='Required';if(!form.phone)e.phone='Required';return e;};
  const handleSave=async()=>{
    const e=validate();
    if(Object.keys(e).length){setErrors(e);return;}
    const payload = {
      vendorName: form.vendorName,
      contactPerson: form.contactPerson || null,
      phone: form.phone,
      email: form.email || null,
      address: form.address || null,
      isActive: form.status === 'Active',
    };
    try {
      if(editing){
        await api.updateVendor(editing.id, payload);
        show('Vendor updated');
      }else{
        await api.createVendor(payload);
        show('Vendor added');
      }
      const fresh = await api.getVendors();
      setVendors(fresh);
      setModal(false);
      setEditing(null);
      setForm(empty);
      setErrors({});
    } catch {
      show('Failed to save vendor','error');
    }
  };
  const cols=[
    {label:'Vendor Name',key:'vendorName'},{label:'Contact Person',key:'contactPerson'},
    {label:'Phone',key:'phone'},{label:'Email',key:'email'},
    {label:'Status',render:v=><StatusBadge status={v.status}/>},
    {label:'Actions',render:v=>(
      <div className="vp-action-btns">
        <Button size="sm" variant="ghost" onClick={()=>{setEditing(v);setForm({...v});setErrors({});setModal(true);}}>Edit</Button>
        <Button size="sm" variant="warning" onClick={()=>setConfirm(v)}>Deactivate</Button>
      </div>
    )},
  ];
  return (
    <AdminLayout title="Vendor Management">
      {Toast}
      <PageHeader title="Vendor Management" subtitle={`${vendors.length} vendors`} action={<Button onClick={()=>{setEditing(null);setForm(empty);setErrors({});setModal(true);}}>Add Vendor</Button>}/>
      <Card><SearchBar value={search} onChange={setSearch} placeholder="Search vendors..."/>{loading?<Loader/>:<Table columns={cols} data={filtered}/>}</Card>
      <Modal open={modal} onClose={()=>setModal(false)} title={editing?'Edit Vendor':'Add Vendor'}
        footer={<div style={{display:'flex',gap:8,justifyContent:'flex-end'}}><Button variant="ghost" onClick={()=>setModal(false)}>Cancel</Button><Button onClick={handleSave}>Save</Button></div>}>
        <Input label="Vendor Name" value={form.vendorName} onChange={set('vendorName')} error={errors.vendorName} required placeholder="Company name"/>
        <FormRow><Input label="Contact Person" value={form.contactPerson} onChange={set('contactPerson')} placeholder="Name"/><Input label="Phone" value={form.phone} onChange={set('phone')} error={errors.phone} required placeholder="01-XXXXXXX"/></FormRow>
        <FormRow><Input label="Email" type="email" value={form.email} onChange={set('email')} placeholder="vendor@email.com"/><Select label="Status" value={form.status} onChange={set('status')}><option>Active</option><option>Inactive</option></Select></FormRow>
        <Input label="Address" value={form.address} onChange={set('address')} placeholder="Street, City"/>
      </Modal>
      <ConfirmDialog open={!!confirm} onClose={()=>setConfirm(null)} title="Deactivate Vendor" message={`Deactivate vendor "${confirm?.vendorName}"?`} onConfirm={async()=>{
        if (!confirm) return;
        try {
          await api.deleteVendor(confirm.id);
          const fresh = await api.getVendors();
          setVendors(fresh);
          show('Vendor deactivated');
        } catch {
          show('Failed to deactivate vendor','error');
        }
        setConfirm(null);
      }}/>
    </AdminLayout>
  );
}

function PartsManagement() {
  const [parts,setParts]=useState([]); const [vendors,setVendors]=useState([]); const [loading,setLoading]=useState(true);
  const [search,setSearch]=useState(''); const [filterCat,setFilterCat]=useState('');
  const [modal,setModal]=useState(false); const [editing,setEditing]=useState(null); const [confirm,setConfirm]=useState(null);
  const {show,Toast}=useToast();
  const empty={partName:'',partCode:'',category:'',description:'',unitPrice:'',stockQty:'',reorderLevel:10,vendorId:'',status:'Active'};
  const [form,setForm]=useState(empty); const [errors,setErrors]=useState({});
  useEffect(()=>{Promise.all([api.getParts(),api.getVendors()]).then(([p,v])=>{setParts(p);setVendors(v);}).finally(()=>setLoading(false));}, []);
  const cats=[...new Set(parts.map(p=>p.category))];
  const filtered=parts.filter(p=>{const ms=[p.partName,p.partCode,p.category].some(v=>v?.toLowerCase().includes(search.toLowerCase()));const mc=!filterCat||p.category===filterCat;return ms&&mc;});
  const set=k=>e=>setForm(f=>({...f,[k]:e.target.value}));
  const validate=()=>{const e={};if(!form.partName)e.partName='Required';if(!form.partCode)e.partCode='Required';if(!form.category)e.category='Required';if(!form.unitPrice||form.unitPrice<0)e.unitPrice='Must be ≥ 0';if(form.stockQty===''||form.stockQty<0)e.stockQty='Must be ≥ 0';return e;};
  const handleSave=async()=>{
    const e=validate();
    if(Object.keys(e).length){setErrors(e);return;}
    const payload = {
      partName: form.partName,
      partCode: form.partCode,
      category: form.category,
      description: form.description || '',
      unitPrice: parseFloat(form.unitPrice),
      stockQuantity: parseInt(form.stockQty),
      reorderLevel: parseInt(form.reorderLevel) || 10,
      vendorId: form.vendorId || null,
      isActive: form.status === 'Active',
    };
    try {
      if(editing){
        await api.updatePart(editing.id, payload);
        show('Part updated');
      }else{
        await api.createPart(payload);
        show('Part added');
      }
      const fresh = await api.getParts();
      setParts(fresh);
      setModal(false);
      setEditing(null);
      setForm(empty);
      setErrors({});
    } catch {
      show('Failed to save part','error');
    }
  };
  const cols=[
    {label:'Part',render:p=><div><div className="vp-fw6">{p.partName}</div><div className="vp-text-sm vp-tx3">{p.partCode}</div></div>},
    {label:'Category',key:'category'},
    {label:'Unit Price',render:p=>formatCurrency(p.unitPrice)},
    {label:'Stock',render:p=><span style={{color:p.stockQty<=p.reorderLevel?'var(--error)':'var(--tx)',fontWeight:600}}>{p.stockQty}</span>},
    {label:'Status',render:p=><StatusBadge status={stockStatus(p)}/>},
    {label:'Vendor',key:'vendorName'},
    {label:'Actions',render:p=>(
      <div className="vp-action-btns">
        <Button size="sm" variant="ghost" onClick={()=>{setEditing(p);setForm({...p});setErrors({});setModal(true);}}>Edit</Button>
        <Button size="sm" variant="warning" onClick={()=>setConfirm(p)}>Deactivate</Button>
      </div>
    )},
  ];
  return (
    <AdminLayout title="Parts Management">
      {Toast}
      <PageHeader title="Parts Management" subtitle={`${parts.length} parts`} action={<Button onClick={()=>{setEditing(null);setForm(empty);setErrors({});setModal(true);}}>Add Part</Button>}/>
      <Card>
        <div className="vp-toolbar">
          <SearchBar value={search} onChange={setSearch} placeholder="Search parts..."/>
          <select className="vp-input vp-filter-sel" value={filterCat} onChange={e=>setFilterCat(e.target.value)}><option value="">All Categories</option>{cats.map(c=><option key={c}>{c}</option>)}</select>
        </div>
        {loading?<Loader/>:<Table columns={cols} data={filtered} emptyMessage="No parts found."/>}
      </Card>
      <Modal open={modal} onClose={()=>setModal(false)} title={editing?'Edit Part':'Add Part'} size="lg"
        footer={<div style={{display:'flex',gap:8,justifyContent:'flex-end'}}><Button variant="ghost" onClick={()=>setModal(false)}>Cancel</Button><Button onClick={handleSave}>Save</Button></div>}>
        <FormRow><Input label="Part Name" value={form.partName} onChange={set('partName')} error={errors.partName} required placeholder="e.g. Brake Pad Set"/><Input label="Part Code" value={form.partCode} onChange={set('partCode')} error={errors.partCode} required placeholder="e.g. BP-001"/></FormRow>
        <FormRow><Input label="Category" value={form.category} onChange={set('category')} error={errors.category} required placeholder="e.g. Brakes"/><Select label="Vendor" value={form.vendorId} onChange={set('vendorId')}><option value="">Select vendor</option>{vendors.filter(v=>v.status==='Active').map(v=><option key={v.id} value={v.id}>{v.vendorName}</option>)}</Select></FormRow>
        <FormRow><Input label="Unit Price (Rs)" type="number" value={form.unitPrice} onChange={set('unitPrice')} error={errors.unitPrice} required min="0" step="0.01"/><Input label="Stock Quantity" type="number" value={form.stockQty} onChange={set('stockQty')} error={errors.stockQty} required min="0"/></FormRow>
        <FormRow><Input label="Reorder Level" type="number" value={form.reorderLevel} onChange={set('reorderLevel')} min="0"/><Select label="Status" value={form.status} onChange={set('status')}><option>Active</option><option>Inactive</option></Select></FormRow>
        <Textarea label="Description" value={form.description} onChange={set('description')} placeholder="Part description..."/>
      </Modal>
      <ConfirmDialog open={!!confirm} onClose={()=>setConfirm(null)} title="Deactivate Part" message={`Deactivate "${confirm?.partName}"?`} onConfirm={async()=>{
        if (!confirm) return;
        try {
          await api.deletePart(confirm.id);
          const fresh = await api.getParts();
          setParts(fresh);
          show('Part deactivated');
        } catch {
          show('Failed to deactivate part','error');
        }
        setConfirm(null);
      }}/>
    </AdminLayout>
  );
}

function PurchaseInvoices() {
  const [invoices,setInvoices]=useState([]); const [vendors,setVendors]=useState([]); const [parts,setParts]=useState([]);
  const [loading,setLoading]=useState(true); const [modal,setModal]=useState(false);
  const {show,Toast}=useToast();
  const emptyForm={vendorId:'',invoiceDate:new Date().toISOString().split('T')[0],notes:''};
  const [form,setForm]=useState(emptyForm); const [items,setItems]=useState([{partId:'',quantity:1,unitCost:''}]);
  const set=k=>e=>setForm(f=>({...f,[k]:e.target.value}));
  useEffect(()=>{Promise.all([api.getPurchaseInvoices(),api.getVendors(),api.getParts()]).then(([inv,v,p])=>{setInvoices(inv);setVendors(v);setParts(p);}).finally(()=>setLoading(false));}, []);
  const setItem=(i,k,v)=>setItems(rows=>rows.map((r,idx)=>{if(idx!==i)return r;const u={...r,[k]:v};if(k==='partId'){const p=parts.find(p=>p.id==v);if(p)u.unitCost=p.unitPrice;}return u;}));
  const lineTotal=item=>parseFloat(item.quantity||0)*parseFloat(item.unitCost||0);
  const grandTotal=items.reduce((s,i)=>s+lineTotal(i),0);
  const handleSave=async()=>{
    const vendor=vendors.find(v=>v.id==form.vendorId);
    if(!vendor){show('Select a vendor','error');return;}
    if(!items.some(i=>i.partId)){show('Add at least one part','error');return;}
    try {
      await api.createPurchaseInvoice({
        vendorId: form.vendorId,
        notes: form.notes || '',
        items: items
          .filter(i => i.partId)
          .map(i => ({ partId: i.partId, quantity: parseInt(i.quantity), unitCost: parseFloat(i.unitCost) || 0 })),
      });
      const [inv, p] = await Promise.all([api.getPurchaseInvoices(), api.getParts()]);
      setInvoices(inv);
      setParts(p);
      setModal(false);
      setForm(emptyForm);
      setItems([{partId:'',quantity:1,unitCost:''}]);
      show('Purchase invoice created');
    } catch {
      show('Failed to create purchase invoice','error');
    }
  };
  const cols=[{label:'Invoice #',key:'invoiceNumber'},{label:'Vendor',key:'vendorName'},{label:'Date',render:i=>formatDate(i.date)},{label:'Total Amount',render:i=><span className="vp-fw6">{formatCurrency(i.totalAmount)}</span>},{label:'Notes',render:i=>i.notes||'—'}];
  return (
    <AdminLayout title="Purchase Invoices">
      {Toast}
      <PageHeader title="Purchase Invoices" subtitle="Stock purchase records" action={<Button onClick={()=>setModal(true)}>New Invoice</Button>}/>
      <Card>{loading?<Loader/>:<Table columns={cols} data={invoices} emptyMessage="No purchase invoices."/>}</Card>
      <Modal open={modal} onClose={()=>setModal(false)} title="Create Purchase Invoice" size="xl"
        footer={<div style={{display:'flex',gap:8,justifyContent:'flex-end'}}><Button variant="ghost" onClick={()=>setModal(false)}>Cancel</Button><Button onClick={handleSave}>Create Invoice</Button></div>}>
        <FormRow><Select label="Vendor" required value={form.vendorId} onChange={set('vendorId')}><option value="">Select vendor</option>{vendors.filter(v=>v.status==='Active').map(v=><option key={v.id} value={v.id}>{v.vendorName}</option>)}</Select><Input label="Invoice Date" type="date" value={form.invoiceDate} onChange={set('invoiceDate')}/></FormRow>
        <div className="vp-invoice-items">
          <div className="vp-invoice-header"><span style={{flex:3}}>Part</span><span style={{flex:1}}>Qty</span><span style={{flex:1}}>Unit Cost</span><span style={{flex:1}}>Total</span><span style={{width:36}}></span></div>
          {items.map((item,i)=>(
            <div key={i} className="vp-invoice-row">
              <div style={{flex:3}}><select className="vp-input" value={item.partId} onChange={e=>setItem(i,'partId',e.target.value)}><option value="">Select part</option>{parts.map(p=><option key={p.id} value={p.id}>{p.partName} ({p.partCode})</option>)}</select></div>
              <div style={{flex:1}}><input type="number" className="vp-input" value={item.quantity} min="1" onChange={e=>setItem(i,'quantity',e.target.value)}/></div>
              <div style={{flex:1}}><input type="number" className="vp-input" value={item.unitCost} min="0" onChange={e=>setItem(i,'unitCost',e.target.value)}/></div>
              <div style={{flex:1,fontWeight:600,color:'var(--tx)'}}>{formatCurrency(lineTotal(item))}</div>
              <button className="vp-remove-row" onClick={()=>setItems(r=>r.filter((_,idx)=>idx!==i))}>×</button>
            </div>
          ))}
          <div style={{padding:'8px 12px'}}><Button size="sm" variant="ghost" onClick={()=>setItems(r=>[...r,{partId:'',quantity:1,unitCost:''}])}>+ Add Row</Button></div>
        </div>
        <div className="vp-invoice-total">Grand Total: {formatCurrency(grandTotal)}</div>
        <Textarea label="Notes" value={form.notes} onChange={set('notes')} placeholder="Optional notes..." rows={2}/>
      </Modal>
    </AdminLayout>
  );
}

function FinancialReports() {
  const [report,setReport]=useState(null); const [loading,setLoading]=useState(true);
  const [period,setPeriod]=useState('monthly'); const [fromDate,setFromDate]=useState(''); const [toDate,setToDate]=useState('');
  useEffect(()=>{setLoading(true);api.getFinancialReport(period).then(setReport).finally(()=>setLoading(false));}, [period]);
  return (
    <AdminLayout title="Financial Reports">
      <PageHeader title="Financial Reports" subtitle="Business performance summary"/>
      <Card>
        <div className="vp-toolbar">
          <Select label="" value={period} onChange={e=>setPeriod(e.target.value)}><option value="daily">Daily</option><option value="monthly">Monthly</option><option value="yearly">Yearly</option></Select>
          <Input label="" type="date" value={fromDate} onChange={e=>setFromDate(e.target.value)}/>
          <Input label="" type="date" value={toDate} onChange={e=>setToDate(e.target.value)}/>
          <Button onClick={()=>{setLoading(true);api.getFinancialReport(period).then(setReport).finally(()=>setLoading(false));}}>Generate</Button>
        </div>
      </Card>
      {loading?<Loader/>:report&&(
        <>
          <div className="vp-dash-grid">
            <DashboardCard title="Total Sales"     value={formatCurrency(report.totalSales)}     accent="var(--success)"/>
            <DashboardCard title="Total Purchases" value={formatCurrency(report.totalPurchases)} accent="#6366f1"/>
            <DashboardCard title="Discount Given"  value={formatCurrency(report.discountGiven)}  accent="var(--warning)"/>
            <DashboardCard title="Credit Sales"    value={formatCurrency(report.creditSales)}    accent="#f97316"/>
            <DashboardCard title="Net Revenue"     value={formatCurrency(report.netRevenue)}     sub={`${report.invoiceCount} invoices`} accent="var(--accent)"/>
          </div>
          <Card>
            <div className="vp-section-title">Summary</div>
            <Table columns={[{label:'Metric',render:r=>r.label},{label:'Amount',render:r=><span className="vp-fw6 vp-tx">{r.value}</span>}]}
              data={[
                {label:'Total Sales Revenue',value:formatCurrency(report.totalSales)},
                {label:'Total Purchases Cost',value:formatCurrency(report.totalPurchases)},
                {label:'Discount Given',value:formatCurrency(report.discountGiven)},
                {label:'Credit Sales',value:formatCurrency(report.creditSales)},
                {label:'Net Revenue',value:formatCurrency(report.netRevenue)},
                {label:'Number of Invoices',value:report.invoiceCount},
              ]}/>
          </Card>
        </>
      )}
    </AdminLayout>
  );
}

function LowStockAlerts() {
  const [parts,setParts]=useState([]); const [loading,setLoading]=useState(true);
  useEffect(()=>{api.getLowStockParts().then(setParts).finally(()=>setLoading(false));}, []);
  const cols=[
    {label:'Part',render:p=><div><div className="vp-fw6">{p.partName}</div><div className="vp-text-sm vp-tx3">{p.partCode}</div></div>},
    {label:'Category',key:'category'},
    {label:'Current Stock',render:p=><span style={{color:p.stockQty===0?'var(--error)':'var(--warning)',fontWeight:700}}>{p.stockQty}</span>},
    {label:'Reorder Level',key:'reorderLevel'},
    {label:'Vendor',key:'vendorName'},
    {label:'Status',render:p=><StatusBadge status={stockStatus(p)}/>},
    {label:'Action',render:()=><Button size="sm" variant="primary" onClick={()=>navigate('/admin/purchase-invoices')}>Order Stock</Button>},
  ];
  return (
    <AdminLayout title="Low Stock Alerts">
      <PageHeader title="Low Stock Alerts" subtitle={`${parts.length} parts need attention`}/>
      {parts.length>0&&<Alert type="warning" message={`${parts.length} part(s) are below reorder level or out of stock.`}/>}
      <Card style={{marginTop:16}}>{loading?<Loader/>:<Table columns={cols} data={parts} emptyMessage="All parts are adequately stocked."/>}</Card>
    </AdminLayout>
  );
}

function AdminNotifications() {
  const [notifications,setNotifications]=useState([]); const [loading,setLoading]=useState(true);
  useEffect(()=>{api.getNotifications().then(setNotifications).finally(()=>setLoading(false));}, []);
  const markRead=async(id)=>{
    try {
      await api.markNotificationRead(id);
      const fresh = await api.getNotifications();
      setNotifications(fresh);
    } catch {
      // Keep UI responsive even if API fails
      setNotifications(n=>n.map(x=>x.id===id?{...x,read:true}:x));
    }
  };
  const typeColors={'Low Stock':'yellow','Credit Reminder':'orange','General':'blue'};
  return (
    <AdminLayout title="Notifications">
      <PageHeader title="Notifications" subtitle={`${notifications.filter(n=>!n.read).length} unread`}
        action={<Button variant="ghost" size="sm" onClick={async()=>{
          const unread = notifications.filter(n=>!n.read);
          for (const n of unread) {
            await markRead(n.id);
          }
        }}>Mark all read</Button>}/>
      <Card>
        {loading?<Loader/>:notifications.length===0?<EmptyState message="No notifications"/>:(
          <div className="vp-notif-full-list">
            {notifications.map(n=>(
              <div key={n.id} className={`vp-notif-full-item ${!n.read?'vp-unread':''}`}>
                <div className="vp-notif-full-left">
                  <Badge color={typeColors[n.type]||'grey'}>{n.type}</Badge>
                  <div className="vp-notif-full-title">{n.title}</div>
                  <div className="vp-notif-full-msg">{n.message}</div>
                  <div className="vp-notif-full-time">{formatDateTime(n.createdAt)}</div>
                </div>
                {!n.read&&<Button size="sm" variant="ghost" onClick={()=>markRead(n.id)}>Mark read</Button>}
              </div>
            ))}
          </div>
        )}
      </Card>
    </AdminLayout>
  );
}

export { AdminDashboard, StaffManagement, VendorManagement, PartsManagement, PurchaseInvoices, FinancialReports, LowStockAlerts, AdminNotifications  };
