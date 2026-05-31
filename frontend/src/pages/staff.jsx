import { StaffLayout, AdminLayout, NavIcon } from '../components/layout.jsx';
import { Icon, Button, Input, Select, Badge, StatusBadge, Card, DashboardCard, Loader, EmptyState, PageHeader, SearchBar, Alert, Modal, Table, FormRow, useToast } from '../components/common.jsx';
import { auth, api, navigate, getPath, formatCurrency, formatDate, formatDateTime, DEMO } from '../utils.js';
import { exportElementToPdf } from '../pdf.js';
// ── Staff Pages ───────────────────────────────────────────────
import React, { useState, useEffect } from 'react';

function StaffDashboard() {
  const [stats,setStats]=useState({ totalSalesToday:0, customersToday:0, todaySales:0, pendingCredit:0 });
  const [invoices,setInvoices]=useState([]); const [loading,setLoading]=useState(true);
  useEffect(()=>{
    Promise.all([api.getStaffStats(), api.getSalesInvoices()])
      .then(([s,inv])=>{ setStats(s||{}); setInvoices((inv||[]).slice(0,4)); })
      .catch(()=>{})
      .finally(()=>setLoading(false));
  }, []);
  if(loading) return <StaffLayout title="Dashboard"><Loader/></StaffLayout>;
  return (
    <StaffLayout title="Dashboard">
      <PageHeader title="Dashboard" subtitle="Staff operations overview"/>
      <div className="vp-dash-grid">
        <DashboardCard title="Customers Today"  value={stats.customersToday}                       accent="var(--accent)"/>
        <DashboardCard title="Sales Today"      value={stats.totalSalesToday}                      accent="var(--success)"/>
        <DashboardCard title="Today's Revenue"  value={formatCurrency(stats.todaySales)}    accent="#6366f1"/>
        <DashboardCard title="Pending Credits"  value={formatCurrency(stats.pendingCredit)} accent="var(--error)"/>
      </div>
      <div className="vp-dash-row">
        <Card className="vp-flex2">
          <div className="vp-section-title">Recent Invoices</div>
          <Table columns={[
            {label:'Invoice #',key:'invoiceNumber'},
            {label:'Customer',key:'customerName'},
            {label:'Date',render:i=>formatDate(i.date)},
            {label:'Total',render:i=><span className="vp-fw6">{formatCurrency(i.totalAmount)}</span>},
            {label:'Status',render:i=><StatusBadge status={i.paymentStatus}/>},
          ]} data={invoices}/>
        </Card>
        <Card className="vp-flex1">
          <div className="vp-section-title">Quick Actions</div>
          <div className="vp-quick-actions">
            <a href="#/staff/customers/register" className="vp-quick-btn"><NavIcon name="add_user" size={15}/> Register Customer</a>
            <a href="#/staff/customers/search"   className="vp-quick-btn"><NavIcon name="search"   size={15}/> Search Customer</a>
            <a href="#/staff/sales"              className="vp-quick-btn"><NavIcon name="sale"     size={15}/> Create Sale</a>
            <a href="#/staff/invoices"           className="vp-quick-btn"><NavIcon name="invoice"  size={15}/> View Invoices</a>
          </div>
        </Card>
      </div>
    </StaffLayout>
  );
}

function CustomerRegistration() {
  const defaultPassword = `Customer@${new Date().getFullYear()}`;
  const [form,setForm]=useState({fullName:'',email:'',phone:'',address:'',password:defaultPassword,confirmPassword:defaultPassword,vehicleNumber:'',vehicleType:'Car',vehicleBrand:'',vehicleModel:'',manufacturedYear:''});
  const [errors,setErrors]=useState({}); const [loading,setLoading]=useState(false);
  const {show,Toast}=useToast();
  const set=k=>e=>setForm(f=>({...f,[k]:e.target.value}));
  const validate = () => {
    const e = {};
    if (!form.fullName) {
      e.fullName = 'Required';
    } else if (form.fullName.trim().length < 3) {
      e.fullName = 'Name must be at least 3 characters';
    }

    const cleanPhone = form.phone.replace(/[^\d+]/g, '').replace(/^\+?977/, '').replace(/^0/, '');
    if (!form.phone) {
      e.phone = 'Required';
    } else if (!/^\d{10}$/.test(cleanPhone)) {
      e.phone = 'Invalid Nepal phone number (must be 10 digits)';
    }

    if (form.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
      e.email = 'Invalid email address';
    }

    if (!form.password) {
      e.password = 'Required';
    } else if (form.password.length < 6) {
      e.password = 'Min. 6 characters';
    }

    if (form.password !== form.confirmPassword) {
      e.confirmPassword = 'Passwords do not match';
    }

    if (!form.vehicleNumber) {
      e.vehicleNumber = 'Required';
    } else if (form.vehicleNumber.trim().length < 4) {
      e.vehicleNumber = 'Invalid vehicle number';
    }
    return e;
  };
  const handleSubmit=async e=>{
    e.preventDefault();
    const errs=validate();
    if(Object.keys(errs).length){setErrors(errs);return;}
    setLoading(true);
    try {
      await api.createCustomer({
        fullName: form.fullName,
        phone: form.phone,
        email: form.email || null,
        address: form.address || null,
        password: form.password,
        vehicleNumber: form.vehicleNumber,
        vehicleType: form.vehicleType,
        vehicleBrand: form.vehicleBrand || 'Unknown',
        vehicleModel: form.vehicleModel || '',
      });
      show('Customer registered successfully');
      setForm({fullName:'',email:'',phone:'',address:'',password:defaultPassword,confirmPassword:defaultPassword,vehicleNumber:'',vehicleType:'Car',vehicleBrand:'',vehicleModel:'',manufacturedYear:''});
      setErrors({});
    } catch {
      show('Failed to register customer','error');
    } finally {
      setLoading(false);
    }
  };
  return (
    <StaffLayout title="Register Customer">
      {Toast}
      <PageHeader title="Register Customer" subtitle="Add new customer with vehicle details"/>
      <Card>
        <form onSubmit={handleSubmit}>
          <p className="vp-section-label">Customer Information</p>
          <FormRow><Input label="Full Name" value={form.fullName} onChange={set('fullName')} error={errors.fullName} required placeholder="Customer's full name"/><Input label="Phone Number" value={form.phone} onChange={set('phone')} error={errors.phone} required placeholder="98XXXXXXXX"/></FormRow>
          <FormRow><Input label="Email Address" type="email" value={form.email} onChange={set('email')} error={errors.email} placeholder="customer@email.com"/><Input label="Address" value={form.address} onChange={set('address')} placeholder="City, District"/></FormRow>
          <FormRow><Input label="Account Password" type="password" value={form.password} onChange={set('password')} error={errors.password} required/><Input label="Confirm Password" type="password" value={form.confirmPassword} onChange={set('confirmPassword')} error={errors.confirmPassword} required/></FormRow>
          <p className="vp-section-label" style={{marginTop:20}}>Vehicle Information</p>
          <FormRow><Input label="Vehicle Number" value={form.vehicleNumber} onChange={set('vehicleNumber')} error={errors.vehicleNumber} required placeholder="BA 1 PA 1234"/><Select label="Vehicle Type" value={form.vehicleType} onChange={set('vehicleType')}>{['Car','SUV','Truck','Van','Motorcycle','Other'].map(t=><option key={t}>{t}</option>)}</Select></FormRow>
          <FormRow><Input label="Brand" value={form.vehicleBrand} onChange={set('vehicleBrand')} placeholder="Toyota, Honda..."/><Input label="Model" value={form.vehicleModel} onChange={set('vehicleModel')} placeholder="Vitz, City..."/><Input label="Year" type="number" value={form.manufacturedYear} onChange={set('manufacturedYear')} placeholder="2020" min="1990" max="2025"/></FormRow>
          <div className="vp-form-actions"><Button type="submit" disabled={loading}>{loading?'Registering...':'Register Customer'}</Button><Button variant="ghost" type="button" onClick={()=>{setForm({fullName:'',email:'',phone:'',address:'',password:defaultPassword,confirmPassword:defaultPassword,vehicleNumber:'',vehicleType:'Car',vehicleBrand:'',vehicleModel:'',manufacturedYear:''});setErrors({});}}>Clear</Button></div>
        </form>
      </Card>
    </StaffLayout>
  );
}

function CustomersPage() {
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');

  useEffect(() => {
    api.getCustomers().then(setCustomers).finally(() => setLoading(false));
  }, []);

  const filtered = customers.filter(c =>
    [c.fullName, c.phone, c.email, c.address].some(v =>
      (v || '').toLowerCase().includes(search.toLowerCase())
    )
  );

  return (
    <StaffLayout title="Customers">
      <PageHeader title="Customers" subtitle="Customer profiles with linked vehicles" />
      <Card>
        <SearchBar value={search} onChange={setSearch} placeholder="Search by name, phone, email..." />
        {loading ? <Loader /> : (
          <Table
            columns={[
              { label: 'Name', key: 'fullName' },
              { label: 'Phone', key: 'phone' },
              { label: 'Email', render: c => c.email || '—' },
              { label: 'Address', render: c => c.address || '—' },
              {
                label: 'Linked Vehicles',
                render: c => (c.vehicles?.length
                  ? c.vehicles.map(v => (
                      <div key={v.id} className="vp-text-sm vp-tx2">
                        {v.vehicleNumber} — {v.brand} {v.model}
                      </div>
                    ))
                  : '—')
              },
              {
                label: 'Action',
                render: c => (
                  <Button size="sm" onClick={() => { navigate('/staff/customers/' + c.id); window.location.reload(); }}>
                    View Profile
                  </Button>
                )
              }
            ]}
            data={filtered}
            emptyMessage="No customers found."
          />
        )}
      </Card>
    </StaffLayout>
  );
}

function CustomerSearch() {
  const [query,setQuery]=useState(''); const [type,setType]=useState('name');
  const [results,setResults]=useState([]); const [searched,setSearched]=useState(false); const [loading,setLoading]=useState(false);
  const handleSearch=async()=>{if(!query.trim())return;setLoading(true);const [customers,vehicles]=await Promise.all([api.getCustomers(),api.getVehicles()]);let found=[];if(type==='name')found=customers.filter(c=>c.fullName.toLowerCase().includes(query.toLowerCase()));else if(type==='phone')found=customers.filter(c=>c.phone.includes(query));else if(type==='id')found=customers.filter(c=>c.id==query);else if(type==='vehicle'){const mv=vehicles.filter(v=>v.vehicleNumber.toLowerCase().includes(query.toLowerCase()));const ids=new Set(mv.map(v=>v.customerId));found=customers.filter(c=>ids.has(c.id));}const withV=found.map(c=>({...c,vehicles:vehicles.filter(v=>v.customerId===c.id)}));setResults(withV);setSearched(true);setLoading(false);};
  return (
    <StaffLayout title="Customer Search">
      <PageHeader title="Customer Search" subtitle="Find customers by name, phone, ID or vehicle number"/>
      <Card>
        <div className="vp-toolbar">
          <select className="vp-input vp-filter-sel" value={type} onChange={e=>setType(e.target.value)}><option value="name">By Name</option><option value="phone">By Phone</option><option value="id">By Customer ID</option><option value="vehicle">By Vehicle Number</option></select>
          <div className="vp-search" style={{flex:1}}>
            <span className="vp-search-icon"><Icon name="search" size={15}/></span>
            <input type="text" className="vp-search-input" value={query} onChange={e=>setQuery(e.target.value)} onKeyDown={e=>e.key==='Enter'&&handleSearch()} placeholder="Enter search value..."/>
          </div>
          <Button onClick={handleSearch}>Search</Button>
        </div>
      </Card>
      {loading&&<Loader/>}
      {searched&&!loading&&(
        <Card style={{marginTop:16}}>
          <div className="vp-section-title">{results.length} result(s) found</div>
          {results.length===0?<EmptyState message="No customers found matching your search."/>:(
            <Table columns={[
              {label:'Name',key:'fullName'},{label:'Phone',key:'phone'},{label:'Email',render:c=>c.email||'—'},
              {label:'Vehicles',render:c=>c.vehicles?.map(v=><div key={v.id} className="vp-text-sm vp-tx2">{v.vehicleNumber} — {v.brand} {v.model}</div>)},
              {label:'Action',render:c=><Button size="sm" onClick={()=>{navigate('/staff/customers/'+c.id);window.location.reload();}}>View Details</Button>},
            ]} data={results}/>
          )}
        </Card>
      )}
    </StaffLayout>
  );
}

function CustomerDetails() {
  const [customer,setCustomer]=useState(null); const [vehicles,setVehicles]=useState([]); const [history,setHistory]=useState({purchases:[],appointments:[]});
  const [loading,setLoading]=useState(true); const [tab,setTab]=useState('info');
  useEffect(()=>{
    const path=getPath();
    const id=path.split('/').pop();
    Promise.all([api.getCustomers(),api.getVehicles(id),api.getCustomerHistory(id)])
      .then(([customers,v,h])=>{
        setCustomer(customers.find(c=>c.id===id)||null);
        setVehicles(v);
        setHistory(h);
      })
      .finally(()=>setLoading(false));
  }, []);
  if(loading) return <StaffLayout title="Customer Details"><Loader/></StaffLayout>;
  if(!customer) return <StaffLayout title="Customer Details"><Alert type="error" message="Customer not found."/></StaffLayout>;
  return (
    <StaffLayout title="Customer Details">
      <PageHeader title={customer.fullName} subtitle={`Phone: ${customer.phone}`} action={<Button variant="ghost" onClick={()=>navigate('/staff/customers/search')}>Back to Search</Button>}/>
      <div className="vp-tab-bar">
        {[['info','Profile'],['vehicles','Vehicles'],['purchases','Purchases'],['appointments','Appointments']].map(([key,label])=>(
          <button key={key} className={`vp-tab-btn ${tab===key?'vp-tab-active':''}`} onClick={()=>setTab(key)}>{label}</button>
        ))}
      </div>
      {tab==='info'&&<Card><div className="vp-detail-grid"><div className="vp-detail-field"><span className="vp-detail-label">Full Name</span><span className="vp-detail-value">{customer.fullName}</span></div><div className="vp-detail-field"><span className="vp-detail-label">Email</span><span className="vp-detail-value">{customer.email||'—'}</span></div><div className="vp-detail-field"><span className="vp-detail-label">Phone</span><span className="vp-detail-value">{customer.phone}</span></div><div className="vp-detail-field"><span className="vp-detail-label">Address</span><span className="vp-detail-value">{customer.address||'—'}</span></div><div className="vp-detail-field"><span className="vp-detail-label">Credit Balance</span><span className="vp-detail-value" style={{color:customer.creditBalance>0?'var(--error)':'var(--tx)',fontWeight:600}}>{formatCurrency(customer.creditBalance)}</span></div></div></Card>}
      {tab==='vehicles'&&<Card><Table columns={[{label:'Vehicle Number',key:'vehicleNumber'},{label:'Type',key:'vehicleType'},{label:'Brand',key:'brand'},{label:'Model',key:'model'},{label:'Year',key:'year'}]} data={vehicles} emptyMessage="No vehicles registered."/></Card>}
      {tab==='purchases'&&<Card><Table columns={[{label:'Invoice #',key:'invoiceNumber'},{label:'Date',render:i=>formatDate(i.date)},{label:'Total',render:i=><span className="vp-fw6">{formatCurrency(i.totalAmount)}</span>},{label:'Status',render:i=><StatusBadge status={i.paymentStatus}/>}]} data={history.purchases} emptyMessage="No purchase history."/></Card>}
      {tab==='appointments'&&<Card><Table columns={[{label:'Service Type',key:'serviceType'},{label:'Vehicle',key:'vehicleNumber'},{label:'Date',render:a=>formatDate(a.date)},{label:'Status',render:a=><StatusBadge status={a.status}/>}]} data={history.appointments} emptyMessage="No appointments."/></Card>}
    </StaffLayout>
  );
}

function PartsSale() {
  const [customers,setCustomers]=useState([]); const [parts,setParts]=useState([]); const [loading,setLoading]=useState(true);
  const [customerId,setCustomerId]=useState(''); const [paymentStatus,setPaymentStatus]=useState('Paid'); const [creditDueDate,setCreditDueDate]=useState('');
  const [items,setItems]=useState([{partId:'',quantity:1,unitPrice:0,lineTotal:0,availableStock:0,partName:''}]);
  const [successInvoice,setSuccessInvoice]=useState(null);
  const {show,Toast}=useToast();
  useEffect(()=>{Promise.all([api.getCustomers(),api.getParts()]).then(([c,p])=>{setCustomers(c);setParts(p);}).finally(()=>setLoading(false));}, []);
  const setItem=(i,k,v)=>setItems(rows=>rows.map((r,idx)=>{
    if(idx!==i)return r;
    const u={...r,[k]:v};
    if(k==='partId'){
      const p=parts.find(p=>p.id==v);
      u.unitPrice=p?p.unitPrice:0;
      u.availableStock=p?p.stockQty:0;
      u.partName=p?p.partName:'';
      const currentQty = parseInt(u.quantity) || 1;
      u.quantity = Math.max(1, Math.min(currentQty, p ? p.stockQty : 1));
      u.lineTotal = u.unitPrice * u.quantity;
    }
    if(k==='quantity'){
      if (v === '') {
        u.quantity = '';
        u.lineTotal = 0;
      } else {
        const qty = parseInt(v) || 1;
        u.quantity = Math.max(1, Math.min(qty, u.availableStock || 1));
        u.lineTotal = u.unitPrice * u.quantity;
      }
    }
    return u;
  }));
  const subtotal=items.reduce((s,i)=>s+(i.lineTotal||0),0);
  const discount=subtotal>5000?subtotal*0.1:0;
  const finalTotal=subtotal-discount;
  const handleSubmit=async()=>{
    if(!customerId){show('Select a customer','error');return;}
    if(!items.some(i=>i.partId)){show('Add at least one part','error');return;}
    const over=items.find(i=>i.partId&&parseInt(i.quantity)>i.availableStock);
    if(over){show(`Insufficient stock for: ${over.partName}`,'error');return;}
    if (paymentStatus === 'Credit' || paymentStatus === 'Partial') {
      if (!creditDueDate) {
        show('Credit due date is required', 'error');
        return;
      }
      const selectedDate = new Date(`${creditDueDate}T00:00:00`);
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      if (selectedDate < today) {
        show('Credit due date cannot be in the past', 'error');
        return;
      }
    }
    try {
      const created = await api.createSalesInvoice({
        customerId,
        paymentStatus,
        creditDueDate: creditDueDate ? new Date(`${creditDueDate}T00:00:00`).toISOString() : null,
        items: items.filter(i=>i.partId).map(i => ({ partId: i.partId, quantity: parseInt(i.quantity) })),
      });
      const customer=customers.find(c=>c.id==customerId);
      setSuccessInvoice({
        invoiceNumber:   created.invoiceNumber,
        customerName:    customer?.fullName,
        subtotal:        created.subtotal,
        discount:        created.discount,
        discountPercent: created.discountPercent ?? 0,
        finalTotal:      created.totalAmount,
        paymentStatus:   created.paymentStatus,
        date: new Date(created.date || Date.now()).toLocaleDateString(),
      });
      const latestParts = await api.getParts();
      setParts(latestParts);
    } catch {
      show('Failed to complete sale','error');
    }
  };
  if(loading) return <StaffLayout title="New Sale"><Loader/></StaffLayout>;
  if(successInvoice) return (
    <StaffLayout title="Invoice Generated">
      <Card>
        <div className="vp-invoice-print">
          <div className="vp-invoice-print-header"><h2>VehicleParts</h2><p style={{color:'var(--tx2)',marginTop:4}}>Sales Invoice — {successInvoice.invoiceNumber}</p><p style={{color:'var(--tx3)',fontSize:12,marginTop:2}}>{successInvoice.date}</p></div>
          <p style={{marginBottom:16,color:'var(--tx2)'}}>Customer: <strong style={{color:'var(--tx)'}}>{successInvoice.customerName}</strong></p>
          <div className="vp-invoice-print-totals">
            <div className="vp-total-row"><span>Subtotal</span><span>{formatCurrency(successInvoice.subtotal)}</span></div>
            {successInvoice.discount>0&&<div className="vp-total-row vp-success-color"><span>Loyalty Discount ({successInvoice.discountPercent}%)</span><span>- {formatCurrency(successInvoice.discount)}</span></div>}
            <div className="vp-total-row vp-total-final"><span>Final Total</span><span>{formatCurrency(successInvoice.finalTotal)}</span></div>
            <div className="vp-total-row"><span>Payment Status</span><span><StatusBadge status={successInvoice.paymentStatus}/></span></div>
          </div>
          <div className="vp-form-actions" style={{marginTop:20}}>
            <Button onClick={()=>window.print()}><Icon name="print" size={14}/> Print</Button>
            <Button variant="ghost" onClick={()=>{setSuccessInvoice(null);setItems([{partId:'',quantity:1,unitPrice:0,lineTotal:0,availableStock:0,partName:''}]);setCustomerId('');}}>New Sale</Button>
            <Button variant="secondary" onClick={()=>navigate('/staff/invoices')}>View All Invoices</Button>
          </div>
        </div>
      </Card>
    </StaffLayout>
  );
  return (
    <StaffLayout title="New Sale">
      {Toast}
      <PageHeader title="New Parts Sale" subtitle="Create a sales transaction"/>
      <Card>
        <FormRow>
          <Select label="Customer" required value={customerId} onChange={e=>setCustomerId(e.target.value)}><option value="">Select customer</option>{customers.map(c=><option key={c.id} value={c.id}>{c.fullName} — {c.phone}</option>)}</Select>
          <Select label="Payment Status" value={paymentStatus} onChange={e=>setPaymentStatus(e.target.value)}><option>Paid</option><option>Credit</option><option>Partial</option></Select>
          {(paymentStatus==='Credit'||paymentStatus==='Partial')&&<Input label="Credit Due Date" type="date" value={creditDueDate} onChange={e=>setCreditDueDate(e.target.value)} min={new Date().toLocaleDateString('en-CA')} required/>}
        </FormRow>
        {/* Items table */}
        <div style={{marginTop:20,border:'1px solid var(--border)',borderRadius:8,overflow:'hidden'}}>
          {/* Header */}
          <div style={{display:'grid',gridTemplateColumns:'3fr 1fr 1fr 1fr 1fr 40px',gap:0,background:'var(--surface-2,#f9fafb)',borderBottom:'1px solid var(--border)',padding:'8px 14px'}}>
            {['Part','In Stock','Qty','Unit Price','Total',''].map((h,i)=>(
              <span key={i} style={{fontSize:12,fontWeight:600,color:'var(--muted)',textTransform:'uppercase',letterSpacing:.5}}>{h}</span>
            ))}
          </div>
          {/* Rows */}
          {items.map((item,i)=>(
            <div key={i} style={{display:'grid',gridTemplateColumns:'3fr 1fr 1fr 1fr 1fr 40px',gap:0,alignItems:'center',padding:'10px 14px',borderBottom:'1px solid var(--border)',background:'var(--surface)'}}>
              <div style={{paddingRight:8}}>
                <select className="vp-input" value={item.partId} onChange={e=>setItem(i,'partId',e.target.value)} style={{width:'100%'}}>
                  <option value="">Select part</option>
                  {parts.filter(p=>p.stockQty>0).map(p=><option key={p.id} value={p.id}>{p.partName} ({p.partCode})</option>)}
                </select>
              </div>
              <div style={{fontSize:13,fontWeight:600,color:item.partId&&item.availableStock<=item.quantity?'var(--error)':'var(--success)'}}>
                {item.partId ? item.availableStock : '—'}
              </div>
              <div style={{paddingRight:8}}>
                <input type="number" className="vp-input" value={item.quantity} min="1" max={item.availableStock||999} onChange={e=>setItem(i,'quantity',e.target.value)} style={{width:'100%'}}/>
              </div>
              <div style={{fontSize:13,color:'var(--muted)'}}>{formatCurrency(item.unitPrice)}</div>
              <div style={{fontSize:13,fontWeight:700,color:'var(--tx)'}}>{formatCurrency(item.lineTotal)}</div>
              <div style={{display:'flex',justifyContent:'center'}}>
                <button onClick={()=>setItems(r=>r.filter((_,idx)=>idx!==i))} style={{width:28,height:28,borderRadius:4,border:'1px solid var(--border)',background:'transparent',color:'var(--muted)',cursor:'pointer',fontSize:16,lineHeight:1,display:'flex',alignItems:'center',justifyContent:'center'}}>×</button>
              </div>
            </div>
          ))}
          {/* Add row */}
          <div style={{padding:'8px 14px',background:'var(--surface)'}}>
            <Button size="sm" variant="ghost" onClick={()=>setItems(r=>[...r,{partId:'',quantity:1,unitPrice:0,lineTotal:0,availableStock:0,partName:''}])}>+ Add Part</Button>
          </div>
        </div>

        {/* Summary */}
        <div style={{marginTop:16,display:'flex',flexDirection:'column',alignItems:'flex-end',gap:6}}>
          <div style={{display:'flex',gap:32,fontSize:14,color:'var(--muted)'}}>
            <span>Subtotal</span><span style={{minWidth:90,textAlign:'right'}}>{formatCurrency(subtotal)}</span>
          </div>
          {discount>0&&(
            <div style={{display:'flex',gap:32,fontSize:13,color:'var(--success)',fontWeight:500}}>
              <span>Loyalty Discount (10% on orders over Rs 5,000)</span><span style={{minWidth:90,textAlign:'right'}}>- {formatCurrency(discount)}</span>
            </div>
          )}
          <div style={{display:'flex',gap:32,fontSize:16,fontWeight:700,color:'var(--tx)',borderTop:'1px solid var(--border)',paddingTop:8,marginTop:2}}>
            <span>Final Total</span><span style={{minWidth:90,textAlign:'right'}}>{formatCurrency(finalTotal)}</span>
          </div>
        </div>

        <div className="vp-form-actions" style={{marginTop:20}}>
          <Button onClick={handleSubmit} size="lg">Complete Sale</Button>
          <Button variant="ghost" onClick={()=>setItems([{partId:'',quantity:1,unitPrice:0,lineTotal:0,availableStock:0,partName:''}])}>Clear Items</Button>
        </div>
      </Card>
    </StaffLayout>
  );
}

function SalesInvoices() {
  const [invoices,setInvoices]=useState([]); const [loading,setLoading]=useState(true);
  const [search,setSearch]=useState(''); const [selected,setSelected]=useState(null);
  const {show,Toast}=useToast();
  useEffect(()=>{api.getSalesInvoices().then(setInvoices).finally(()=>setLoading(false));}, []);
  const filtered=invoices.filter(i=>[i.invoiceNumber,i.customerName].some(v=>v?.toLowerCase().includes(search.toLowerCase())));
  const sendEmail=async(inv)=>{
    try {
      await api.sendInvoiceEmail(inv.id);
      show(`Invoice ${inv.invoiceNumber} sent to customer`);
      setInvoices(prev=>prev.map(i=>i.id===inv.id?{...i,emailSent:true}:i));
    } catch (err) {
      show('Failed to send email','error');
    }
  };
  const cols=[
    {label:'Invoice #',key:'invoiceNumber'},{label:'Customer',key:'customerName'},
    {label:'Date',render:i=>formatDate(i.date)},
    {label:'Subtotal',render:i=>formatCurrency(i.subtotal)},
    {label:'Discount',render:i=>i.discount>0?<span className="vp-success-color">- {formatCurrency(i.discount)}</span>:'—'},
    {label:'Total',render:i=><span className="vp-fw6">{formatCurrency(i.totalAmount)}</span>},
    {label:'Payment',render:i=><StatusBadge status={i.paymentStatus}/>},
    {label:'Email',render:i=>i.emailSent?<Badge color="green">Sent</Badge>:<Badge color="grey">Pending</Badge>},
    {label:'Actions',render:i=>(
      <div className="vp-action-btns">
        <Button size="sm" variant="ghost" onClick={()=>setSelected(i)}>View</Button>
        {!i.emailSent&&<Button size="sm" variant="secondary" onClick={()=>sendEmail(i)}><Icon name="mail" size={13}/> Send</Button>}
      </div>
    )},
  ];
  return (
    <StaffLayout title="Sales Invoices">
      {Toast}
      <PageHeader title="Sales Invoices" subtitle={`${invoices.length} total invoices`} action={<Button onClick={()=>navigate('/staff/sales')}>New Sale</Button>}/>
      <Card><SearchBar value={search} onChange={setSearch} placeholder="Search by invoice # or customer..."/>{loading?<Loader/>:<Table columns={cols} data={filtered} emptyMessage="No invoices found."/>}</Card>
      <Modal open={!!selected} onClose={()=>setSelected(null)} title={`Invoice — ${selected?.invoiceNumber}`} size="md">
        {selected&&(
          <div>
            <div className="vp-detail-grid"><div className="vp-detail-field"><span className="vp-detail-label">Customer</span><span className="vp-detail-value">{selected.customerName}</span></div><div className="vp-detail-field"><span className="vp-detail-label">Staff</span><span className="vp-detail-value">{selected.staffName}</span></div><div className="vp-detail-field"><span className="vp-detail-label">Date</span><span className="vp-detail-value">{formatDate(selected.date)}</span></div><div className="vp-detail-field"><span className="vp-detail-label">Status</span><span className="vp-detail-value"><StatusBadge status={selected.paymentStatus}/></span></div></div>
            <div className="vp-invoice-print-totals" style={{marginTop:16}}>
              <div className="vp-total-row"><span>Subtotal</span><span>{formatCurrency(selected.subtotal)}</span></div>
              {selected.discount>0&&<div className="vp-total-row vp-success-color"><span>Discount (10%)</span><span>- {formatCurrency(selected.discount)}</span></div>}
              <div className="vp-total-row vp-total-final"><span>Total</span><span>{formatCurrency(selected.totalAmount)}</span></div>
            </div>
            {selected.creditDueDate&&<p style={{marginTop:10,fontSize:13,color:'var(--tx2)'}}>Credit Due: <strong style={{color:'var(--tx)'}}>{formatDate(selected.creditDueDate)}</strong></p>}
            <div className="vp-form-actions" style={{marginTop:18}}>
              <Button onClick={()=>window.print()}><Icon name="print" size={14}/> Print</Button>
              {!selected.emailSent&&<Button variant="secondary" onClick={()=>{sendEmail(selected);setSelected(null);}}><Icon name="mail" size={14}/> Send Email</Button>}
            </div>
          </div>
        )}
      </Modal>
    </StaffLayout>
  );
}

function CustomerReports() {
  const [reportType, setReportType] = useState('high-spenders');
  const [data,       setData]       = useState([]);
  const [filtered,   setFiltered]   = useState([]);
  const [search,     setSearch]     = useState('');
  const [loading,    setLoading]    = useState(false);
  const [generated,  setGenerated]  = useState(false);
  const [error,      setError]      = useState('');
  const [sortDir,    setSortDir]    = useState('desc'); // for primary metric

  // Filter by search
  React.useEffect(() => {
    const q = search.toLowerCase();
    setFiltered(q ? data.filter(r => (r.fullName||'').toLowerCase().includes(q)) : data);
  }, [search, data]);

  const generate = async () => {
    setLoading(true); setGenerated(false); setError(''); setSearch('');
    try {
      let result = [];
      if (reportType === 'high-spenders') {
        const res = await api.getHighSpenders();
        result = (res || []).map(r => ({
          rank:         r.rank        ?? r.Rank        ?? 0,
          customerId:   r.customerId  ?? r.CustomerId  ?? '',
          fullName:     r.fullName    ?? r.FullName    ?? '',
          phone:        r.phone       ?? r.Phone       ?? '',
          totalSpent:   r.totalSpent  ?? r.TotalSpent  ?? 0,
          invoiceCount: r.invoiceCount?? r.InvoiceCount?? 0,
        }));
      } else if (reportType === 'regulars') {
        const res = await api.getRegularCustomers();
        result = (res || []).map(r => ({
          rank:         r.rank        ?? r.Rank        ?? 0,
          customerId:   r.customerId  ?? r.CustomerId  ?? '',
          fullName:     r.fullName    ?? r.FullName    ?? '',
          phone:        r.phone       ?? r.Phone       ?? '',
          invoiceCount: r.invoiceCount?? r.InvoiceCount?? 0,
          totalSpent:   r.totalSpent  ?? r.TotalSpent  ?? 0,
        }));
      } else {
        const res = await api.getCreditOverdue();
        result = (res || []).map(r => ({
          id:           r.id           ?? r.Id           ?? '',
          customerId:   r.customerId   ?? r.CustomerId   ?? '',
          fullName:     r.customerName ?? r.fullName     ?? '',
          phone:        r.customerPhone?? r.phone        ?? '',
          invoiceNumber:r.invoiceNumber?? '',
          invoiceDate:  r.date         ?? r.Date         ?? '',
          pendingAmount:r.totalAmount  ?? r.TotalAmount  ?? 0,
          daysOverdue:  r.daysOverdue  ?? r.DaysOverdue  ?? 0,
        }));
      }
      setData(result);
    } catch {
      setError('Failed to generate report. Please try again.');
      setData([]);
    } finally {
      setLoading(false);
      setGenerated(true);
    }
  };

  const toggleSort = () => {
    const dir = sortDir === 'desc' ? 'asc' : 'desc';
    setSortDir(dir);
    const sortKey = reportType === 'regulars' ? 'invoiceCount' : reportType === 'pending-credits' ? 'daysOverdue' : 'totalSpent';
    setData(prev => [...prev].sort((a, b) => dir === 'desc' ? b[sortKey] - a[sortKey] : a[sortKey] - b[sortKey]));
  };

  const overdueColor = (days) => {
    if (days >= 60) return { color: 'var(--error)', fontWeight: 700 };
    if (days >= 30) return { color: '#f59e0b', fontWeight: 700 };
    return { color: 'var(--tx2)' };
  };

  const sortArrow = sortDir === 'desc' ? ' ↓' : ' ↑';

  const colsMap = {
    'high-spenders': [
      { label: 'Rank',        render: c => <span className="vp-fw6">#{c.rank}</span> },
      { label: 'Customer Name',key: 'fullName' },
      { label: 'Customer ID', render: c => <span className="vp-text-sm vp-tx3" style={{fontFamily:'monospace'}}>{String(c.customerId).slice(0,8)}…</span> },
      { label: 'Phone',       key: 'phone' },
      { label: <span style={{cursor:'pointer'}} onClick={toggleSort}>{'Total Spend' + sortArrow}</span>,
        render: c => <span className="vp-fw6 vp-accent-color">{formatCurrency(c.totalSpent)}</span> },
      { label: 'Invoices',    key: 'invoiceCount' },
    ],
    'regulars': [
      { label: 'Rank',        render: c => <span className="vp-fw6">#{c.rank}</span> },
      { label: 'Customer Name',key: 'fullName' },
      { label: 'Customer ID', render: c => <span className="vp-text-sm vp-tx3" style={{fontFamily:'monospace'}}>{String(c.customerId).slice(0,8)}…</span> },
      { label: 'Phone',       key: 'phone' },
      { label: <span style={{cursor:'pointer'}} onClick={toggleSort}>{'Visit Count' + sortArrow}</span>,
        render: c => <span className="vp-fw6">{c.invoiceCount}</span> },
      { label: 'Total Spend', render: c => formatCurrency(c.totalSpent) },
    ],
    'pending-credits': [
      { label: 'Customer Name',key: 'fullName' },
      { label: 'Phone',        key: 'phone' },
      { label: 'Invoice #',    key: 'invoiceNumber' },
      { label: 'Invoice Date', render: c => formatDate(c.invoiceDate) },
      { label: 'Amount Owed',  render: c => <span className="vp-error-color vp-fw6">{formatCurrency(c.pendingAmount)}</span> },
      { label: <span style={{cursor:'pointer'}} onClick={toggleSort}>{'Days Overdue' + sortArrow}</span>,
        render: c => <span style={overdueColor(c.daysOverdue)}>{c.daysOverdue} days</span> },
    ],
  };

  const downloadPDF = async () => {
    try {
      const element = document.getElementById('customer-report-content');
      await exportElementToPdf({
        element,
        fileName: `Customer_Report_${reportType}.pdf`,
        title: 'Customer Report',
        subtitle: reportLabels[reportType] || 'Customer analytics',
        accent: '#16a34a',
      });
    } catch (e) { console.error('PDF error', e); }
  };

  const reportLabels = { 'high-spenders': 'High Spenders', 'regulars': 'Regular Customers', 'pending-credits': 'Pending Credits' };

  return (
    <StaffLayout title="Customer Reports">
      <PageHeader title="Customer Reports" subtitle="Analyse customer data"/>
      <Card>
        <div className="vp-toolbar">
          <Select label="" value={reportType} onChange={e=>{setReportType(e.target.value);setGenerated(false);setData([]);setSearch('');}}>
            <option value="high-spenders">High Spenders</option>
            <option value="regulars">Regular Customers</option>
            <option value="pending-credits">Pending Credits</option>
          </Select>
          <Button onClick={generate}>Generate</Button>
          {generated && data.length > 0 && <Button onClick={downloadPDF} variant="success">Download PDF</Button>}
        </div>
      </Card>
      {loading && <Loader text="Generating report..."/>}
      {error && <Alert type="error" message={error}/>}
      {generated && !loading && !error && (
        <div id="customer-report-content">
          <Card style={{marginTop:16}}>
            <div style={{display:'flex',alignItems:'center',gap:12,marginBottom:12,flexWrap:'wrap'}}>
              <div className="vp-section-title" style={{margin:0}}>{reportLabels[reportType]} — {filtered.length} record(s)</div>
              <SearchBar value={search} onChange={setSearch} placeholder="Filter by customer name…"/>
            </div>
            {data.length === 0
              ? <EmptyState message={`No ${reportLabels[reportType].toLowerCase()} found.`}/>
              : <Table columns={colsMap[reportType]} data={filtered} emptyMessage="No matching customers."/>
            }
          </Card>
        </div>
      )}
    </StaffLayout>
  );
}

function CreditReminders() {
  const [overdue,  setOverdue]  = useState([]);
  const [loading,  setLoading]  = useState(true);
  const [sent,     setSent]     = useState(new Set());
  const [search,   setSearch]   = useState('');
  const { show, Toast } = useToast();

  const overdueColor = (days) => {
    if (days >= 60) return { color: 'var(--error)', fontWeight: 700 };
    if (days >= 30) return { color: '#f59e0b',      fontWeight: 700 };
    return { color: 'var(--tx2)' };
  };

  const load = async () => {
    setLoading(true);
    try {
      const inv = await api.getCreditOverdue();
      // Backend now sends daysOverdue; fall back to front-end calc for safety
      const enriched = (inv || []).map(i => ({
        ...i,
        customerName: i.customerName ?? '',
        daysOverdue: i.daysOverdue ?? Math.floor((new Date() - new Date(i.creditDueDate)) / 86400000),
      }));
      setOverdue(enriched);
    } catch { setOverdue([]); }
    finally { setLoading(false); }
  };

  useEffect(() => { load(); }, []);

  const handleSendAll = async () => {
    try {
      const res = await api.sendCreditReminders();
      const count = res?.remindersSent ?? res?.data?.remindersSent ?? 0;
      show(`${count} reminder(s) sent successfully`);
      await load();
    } catch { show('Failed to send reminders', 'error'); }
  };

  const handleSendOne = async (inv) => {
    try {
      await api.sendCreditReminder(inv.id);
      show(`Reminder sent to ${inv.customerName}`);
      setSent(prev => new Set([...prev, inv.id]));
    } catch { show('Failed to send reminder', 'error'); }
  };

  const displayed = search
    ? overdue.filter(i => (i.customerName||'').toLowerCase().includes(search.toLowerCase()))
    : overdue;

  const cols = [
    { label: 'Customer',     render: i => <div><div className="vp-fw6">{i.customerName}</div><div className="vp-text-sm vp-tx3">{i.customerPhone||'—'}</div></div> },
    { label: 'Invoice #',    key: 'invoiceNumber' },
    { label: 'Invoice Date', render: i => formatDate(i.date) },
    { label: 'Due Date',     render: i => formatDate(i.creditDueDate) },
    { label: 'Amount Owed',  render: i => <span className="vp-error-color vp-fw6">{formatCurrency(i.totalAmount)}</span> },
    { label: 'Days Overdue', render: i => <span style={overdueColor(i.daysOverdue)}>{i.daysOverdue} days</span> },
    { label: 'Action',       render: i => sent.has(i.id)
        ? <Badge color="green">Sent</Badge>
        : <Button size="sm" variant="secondary" onClick={() => handleSendOne(i)}><Icon name="mail" size={13}/> Send</Button>
    },
  ];

  return (
    <StaffLayout title="Credit Reminders">
      {Toast}
      <PageHeader title="Credit Reminders" subtitle="Customers with overdue payments"
        action={<Button variant="secondary" onClick={handleSendAll}>Send All Reminders</Button>}/>
      {overdue.length > 0 && <Alert type="warning" message={`${overdue.length} customer(s) have overdue credit payments.`}/>}
      <Card style={{marginTop:16}}>
        <div style={{marginBottom:12}}>
          <SearchBar value={search} onChange={setSearch} placeholder="Filter by customer name…"/>
        </div>
        {loading ? <Loader/> : <Table columns={cols} data={displayed} emptyMessage="No overdue credit payments."/>}
      </Card>
    </StaffLayout>
  );
}

function StaffAppointments({ isAdmin = false }) {
  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState('All');
  const [searchQuery, setSearchQuery] = useState('');
  const { show, Toast } = useToast();

  const fetchAppointments = async () => {
    try {
      const data = await api.getAppointments();
      setAppointments(data);
    } catch {
      show('Failed to fetch appointments', 'error');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAppointments();
  }, []);

  const handleStatusChange = async (id, newStatus) => {
    try {
      await api.updateAppointmentStatus(id, newStatus);
      show(`Appointment status updated to ${newStatus}`);
      fetchAppointments();
    } catch (err) {
      show(err.message || 'Failed to update status', 'error');
    }
  };

  const filtered = appointments.filter(a => {
    const matchesStatus = statusFilter === 'All' || a.status === statusFilter;
    const matchesSearch = 
      (a.customerName || '').toLowerCase().includes(searchQuery.toLowerCase()) ||
      (a.vehicleNumber || '').toLowerCase().includes(searchQuery.toLowerCase()) ||
      (a.serviceType || '').toLowerCase().includes(searchQuery.toLowerCase());
    return matchesStatus && matchesSearch;
  });

  const LayoutComponent = isAdmin ? AdminLayout : StaffLayout;

  if (loading) return <LayoutComponent title="Appointments"><Loader /></LayoutComponent>;

  return (
    <LayoutComponent title="Appointments">
      {Toast}
      <PageHeader title="Appointments" subtitle="Manage customer service bookings" />
      <Card style={{ marginBottom: 16 }}>
        <FormRow>
          <div style={{ flex: 2 }}>
            <SearchBar value={searchQuery} onChange={setSearchQuery} placeholder="Search by customer name, vehicle, or service..." />
          </div>
          <div style={{ flex: 1 }}>
            <Select label="Filter by Status" value={statusFilter} onChange={e => setStatusFilter(e.target.value)}>
              <option value="All">All Statuses</option>
              <option value="Pending">Pending</option>
              <option value="Confirmed">Confirmed</option>
              <option value="In Progress">In Progress</option>
              <option value="Completed">Completed</option>
              <option value="Cancelled">Cancelled</option>
            </Select>
          </div>
        </FormRow>
      </Card>

      <Card style={{ marginTop: 16 }}>
        <Table
          columns={[
            { label: 'Customer', key: 'customerName' },
            { label: 'Vehicle Number', key: 'vehicleNumber' },
            { label: 'Service Type', key: 'serviceType' },
            { label: 'Description', key: 'description' },
            { label: 'Preferred Date & Time', render: a => formatDateTime(a.date) },
            { label: 'Status', render: a => <StatusBadge status={a.status} /> },
            {
              label: 'Actions',
              render: a => (
                <div style={{ display: 'flex', gap: 6 }}>
                  {a.status === 'Pending' && (
                    <>
                      <Button size="sm" variant="success" onClick={() => handleStatusChange(a.id, 'Confirmed')}>
                        Confirm
                      </Button>
                      <Button size="sm" variant="danger" onClick={() => handleStatusChange(a.id, 'Cancelled')}>
                        Cancel
                      </Button>
                    </>
                  )}
                  {a.status === 'Confirmed' && (
                    <Button size="sm" variant="primary" onClick={() => handleStatusChange(a.id, 'In Progress')}>
                      Start Service
                    </Button>
                  )}
                  {a.status === 'In Progress' && (
                    <Button size="sm" variant="success" onClick={() => handleStatusChange(a.id, 'Completed')}>
                      Complete Service
                    </Button>
                  )}
                  {a.status !== 'Completed' && a.status !== 'Cancelled' && (
                    <Select
                      value={a.status}
                      onChange={e => handleStatusChange(a.id, e.target.value)}
                      style={{ height: 28, padding: '0 8px', fontSize: 12, width: 'auto' }}
                    >
                      <option value="Pending" disabled>Pending</option>
                      <option value="Confirmed">Confirmed</option>
                      <option value="In Progress">In Progress</option>
                      <option value="Completed">Completed</option>
                      <option value="Cancelled">Cancelled</option>
                    </Select>
                  )}
                </div>
              ),
            },
          ]}
          data={filtered}
          emptyMessage="No matching appointments found."
        />
      </Card>
    </LayoutComponent>
  );
}

export { StaffDashboard, CustomerRegistration, CustomersPage, CustomerSearch, CustomerDetails, PartsSale, SalesInvoices, CustomerReports, CreditReminders, StaffAppointments };
