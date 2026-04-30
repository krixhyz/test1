import { StaffLayout } from '../components/layout.jsx';
import { NavIcon } from '../components/layout.jsx';
import { Icon, Button, Input, Select, Badge, StatusBadge, Card, DashboardCard, Loader, EmptyState, PageHeader, SearchBar, Alert, Modal, Table, FormRow, useToast } from '../components/common.jsx';
import { auth, api, navigate, getPath, formatCurrency, formatDate, formatDateTime, DEMO } from '../utils.js';
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
  const validate=()=>{const e={};if(!form.fullName)e.fullName='Required';if(!form.phone)e.phone='Required';if(form.email&&!/\S+@\S+\.\S+/.test(form.email))e.email='Invalid email';if(!form.password||form.password.length<6)e.password='Min. 6 characters';if(form.password!==form.confirmPassword)e.confirmPassword='Passwords do not match';if(!form.vehicleNumber)e.vehicleNumber='Required';return e;};
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
  const setItem=(i,k,v)=>setItems(rows=>rows.map((r,idx)=>{if(idx!==i)return r;const u={...r,[k]:v};if(k==='partId'){const p=parts.find(p=>p.id==v);u.unitPrice=p?p.unitPrice:0;u.availableStock=p?p.stockQty:0;u.partName=p?p.partName:'';u.lineTotal=u.unitPrice*u.quantity;}if(k==='quantity')u.lineTotal=u.unitPrice*parseFloat(v||0);return u;}));
  const subtotal=items.reduce((s,i)=>s+(i.lineTotal||0),0);
  const discount=subtotal>5000?subtotal*0.1:0;
  const finalTotal=subtotal-discount;
  const handleSubmit=async()=>{
    if(!customerId){show('Select a customer','error');return;}
    if(!items.some(i=>i.partId)){show('Add at least one part','error');return;}
    const over=items.find(i=>i.partId&&parseInt(i.quantity)>i.availableStock);
    if(over){show(`Insufficient stock for: ${over.partName}`,'error');return;}
    if((paymentStatus==='Credit'||paymentStatus==='Partial')&&!creditDueDate){show('Credit due date is required','error');return;}
    try {
      const created = await api.createSalesInvoice({
        customerId,
        paymentStatus,
        creditDueDate: creditDueDate ? new Date(`${creditDueDate}T00:00:00`).toISOString() : null,
        items: items.filter(i=>i.partId).map(i => ({ partId: i.partId, quantity: parseInt(i.quantity) })),
      });
      const customer=customers.find(c=>c.id==customerId);
      setSuccessInvoice({
        invoiceNumber: created.invoiceNumber,
        customerName: customer?.fullName,
        subtotal: created.subtotal,
        discount: created.discount,
        finalTotal: created.totalAmount,
        paymentStatus: created.paymentStatus,
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
            {successInvoice.discount>0&&<div className="vp-total-row vp-success-color"><span>Loyalty Discount (10%)</span><span>- {formatCurrency(successInvoice.discount)}</span></div>}
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
          {(paymentStatus==='Credit'||paymentStatus==='Partial')&&<Input label="Credit Due Date" type="date" value={creditDueDate} onChange={e=>setCreditDueDate(e.target.value)} required/>}
        </FormRow>
        <div className="vp-invoice-items" style={{marginTop:16}}>
          <div className="vp-invoice-header"><span style={{flex:3}}>Part</span><span style={{flex:1}}>In Stock</span><span style={{flex:1}}>Qty</span><span style={{flex:1}}>Unit Price</span><span style={{flex:1}}>Total</span><span style={{width:36}}></span></div>
          {items.map((item,i)=>(
            <div key={i} className="vp-invoice-row">
              <div style={{flex:3}}><select className="vp-input" value={item.partId} onChange={e=>setItem(i,'partId',e.target.value)}><option value="">Select part</option>{parts.filter(p=>p.stockQty>0).map(p=><option key={p.id} value={p.id}>{p.partName} ({p.partCode})</option>)}</select></div>
              <div style={{flex:1,color:item.partId&&item.availableStock<=item.quantity?'var(--error)':'var(--success)',fontWeight:600}}>{item.partId?item.availableStock:'—'}</div>
              <div style={{flex:1}}><input type="number" className="vp-input" value={item.quantity} min="1" max={item.availableStock||999} onChange={e=>setItem(i,'quantity',parseInt(e.target.value)||1)}/></div>
              <div style={{flex:1,color:'var(--tx2)'}}>{formatCurrency(item.unitPrice)}</div>
              <div style={{flex:1,fontWeight:700,color:'var(--tx)'}}>{formatCurrency(item.lineTotal)}</div>
              <button className="vp-remove-row" onClick={()=>setItems(r=>r.filter((_,idx)=>idx!==i))}>×</button>
            </div>
          ))}
          <div style={{padding:'8px 12px'}}><Button size="sm" variant="ghost" onClick={()=>setItems(r=>[...r,{partId:'',quantity:1,unitPrice:0,lineTotal:0,availableStock:0,partName:''}])}>+ Add Part</Button></div>
        </div>
        <div className="vp-sale-summary">
          <div className="vp-total-row"><span>Subtotal</span><span>{formatCurrency(subtotal)}</span></div>
          {discount>0&&<div className="vp-total-row vp-success-color"><span>Loyalty Discount (10% on purchase over Rs 5,000)</span><span>- {formatCurrency(discount)}</span></div>}
          <div className="vp-total-row vp-total-final"><span>Final Total</span><span>{formatCurrency(finalTotal)}</span></div>
        </div>
        <div className="vp-form-actions"><Button onClick={handleSubmit} size="lg">Complete Sale</Button><Button variant="ghost" onClick={()=>setItems([{partId:'',quantity:1,unitPrice:0,lineTotal:0,availableStock:0,partName:''}])}>Clear Items</Button></div>
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
  const sendEmail=inv=>{show(`Invoice ${inv.invoiceNumber} sent to customer`);setInvoices(prev=>prev.map(i=>i.id===inv.id?{...i,emailSent:true}:i));};
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
  const [reportType,setReportType]=useState('high-spenders'); const [fromDate,setFromDate]=useState(''); const [toDate,setToDate]=useState('');
  const [data,setData]=useState([]); const [loading,setLoading]=useState(false); const [generated,setGenerated]=useState(false);
  const generate=async()=>{setLoading(true);setGenerated(false);const [customers,invoices]=await Promise.all([api.getCustomers(),api.getSalesInvoices()]);let result=[];if(reportType==='high-spenders'){result=customers.map(c=>({...c,totalSpent:invoices.filter(i=>i.customerId===c.id).reduce((s,i)=>s+i.totalAmount,0),invoiceCount:invoices.filter(i=>i.customerId===c.id).length})).filter(c=>c.totalSpent>0).sort((a,b)=>b.totalSpent-a.totalSpent);}else if(reportType==='regulars'){result=customers.map(c=>({...c,invoiceCount:invoices.filter(i=>i.customerId===c.id).length,totalSpent:invoices.filter(i=>i.customerId===c.id).reduce((s,i)=>s+i.totalAmount,0)})).filter(c=>c.invoiceCount>=1).sort((a,b)=>b.invoiceCount-a.invoiceCount);}else if(reportType==='pending-credits'){result=customers.filter(c=>c.creditBalance>0).map(c=>({...c,pendingAmount:c.creditBalance,invoiceCount:invoices.filter(i=>i.customerId===c.id&&i.paymentStatus==='Credit').length}));}setData(result);setLoading(false);setGenerated(true);};
  const colsMap={'high-spenders':[{label:'Customer',key:'fullName'},{label:'Phone',key:'phone'},{label:'Total Spent',render:c=><span className="vp-fw6 vp-accent-color">{formatCurrency(c.totalSpent)}</span>},{label:'Invoices',key:'invoiceCount'}],'regulars':[{label:'Customer',key:'fullName'},{label:'Phone',key:'phone'},{label:'Visit Count',render:c=><span className="vp-fw6">{c.invoiceCount}</span>},{label:'Total Spent',render:c=>formatCurrency(c.totalSpent)}],'pending-credits':[{label:'Customer',key:'fullName'},{label:'Phone',key:'phone'},{label:'Pending Amount',render:c=><span className="vp-error-color vp-fw6">{formatCurrency(c.pendingAmount)}</span>},{label:'Credit Invoices',key:'invoiceCount'}]};
  return (
    <StaffLayout title="Customer Reports">
      <PageHeader title="Customer Reports" subtitle="Analyse customer data"/>
      <Card>
        <div className="vp-toolbar">
          <Select label="" value={reportType} onChange={e=>setReportType(e.target.value)}><option value="high-spenders">High Spenders</option><option value="regulars">Regular Customers</option><option value="pending-credits">Pending Credits</option></Select>
          <Input label="" type="date" value={fromDate} onChange={e=>setFromDate(e.target.value)}/><Input label="" type="date" value={toDate} onChange={e=>setToDate(e.target.value)}/>
          <Button onClick={generate}>Generate</Button>
        </div>
      </Card>
      {loading&&<Loader text="Generating report..."/>}
      {generated&&!loading&&<Card style={{marginTop:16}}><div className="vp-section-title">{{  'high-spenders':'High Spenders','regulars':'Regular Customers','pending-credits':'Pending Credits'}[reportType]} — {data.length} records</div><Table columns={colsMap[reportType]} data={data} emptyMessage="No data for selected criteria."/></Card>}
    </StaffLayout>
  );
}

function CreditReminders() {
  const [overdue,setOverdue]=useState([]); const [loading,setLoading]=useState(true); const [sent,setSent]=useState(new Set());
  const {show,Toast}=useToast();
  useEffect(()=>{Promise.all([api.getCreditOverdue(),api.getCustomers()]).then(([inv,customers])=>{const enriched=inv.map(i=>{const c=customers.find(c=>c.id===i.customerId);const d=Math.floor((new Date()-new Date(i.creditDueDate))/(1000*60*60*24));return{...i,customerEmail:c?.email,customerPhone:c?.phone,daysOverdue:d};});setOverdue(enriched);}).finally(()=>setLoading(false));}, []);
  const cols=[
    {label:'Customer',key:'customerName'},{label:'Email',render:i=>i.customerEmail||'—'},
    {label:'Invoice #',key:'invoiceNumber'},{label:'Due Date',render:i=>formatDate(i.creditDueDate)},
    {label:'Amount Due',render:i=><span className="vp-error-color vp-fw6">{formatCurrency(i.totalAmount)}</span>},
    {label:'Days Overdue',render:i=><span className="vp-error-color">{i.daysOverdue} days</span>},
    {label:'Action',render:i=>sent.has(i.id)?<Badge color="green">Sent</Badge>:<Button size="sm" onClick={()=>{setSent(prev=>new Set([...prev,i.id]));show(`Reminder sent to ${i.customerName}`);}}><Icon name="mail" size={13}/> Send</Button>},
  ];
  return (
    <StaffLayout title="Credit Reminders">
      {Toast}
      <PageHeader title="Credit Reminders" subtitle="Customers with overdue payments"/>
      {overdue.length>0&&<Alert type="warning" message={`${overdue.length} customer(s) have overdue credit payments.`}/>}
      <Card style={{marginTop:16}}>{loading?<Loader/>:<Table columns={cols} data={overdue} emptyMessage="No overdue credit payments."/>}</Card>
    </StaffLayout>
  );
}

export { StaffDashboard, CustomerRegistration, CustomerSearch, CustomerDetails, PartsSale, SalesInvoices, CustomerReports, CreditReminders  };
