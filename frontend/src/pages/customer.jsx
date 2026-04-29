import { CustomerLayout, NavIcon } from '../components/layout.jsx';
import { Button, Input, Select, Textarea, Badge, StatusBadge, Card, DashboardCard, Loader, EmptyState, PageHeader, Alert, Modal, ConfirmDialog, Table, FormRow, StarRating, useToast } from '../components/common.jsx';
import { auth, api, navigate, getPath, formatCurrency, formatDate, formatDateTime, DEMO } from '../utils.js';
// ── Customer Pages ─────────────────────────────────────────────
import React, { useState, useEffect  } from 'react';

function CustomerDashboard() {
  const user = auth.getUser();
  const [data,setData]=useState({vehicles:[],purchases:[],appointments:[]});
  const [predictions,setPredictions]=useState([]);
  const [loading,setLoading]=useState(true);
  useEffect(()=>{Promise.all([api.getCustomerHistory(1),api.getVehicles(1),api.getPredictions(1)]).then(([h,v,p])=>{setData({...h,vehicles:v});setPredictions(p.slice(0,2));}).finally(()=>setLoading(false));}, []);
  if(loading) return <CustomerLayout title="Dashboard"><Loader/></CustomerLayout>;
  return (
    <CustomerLayout title="Dashboard">
      <PageHeader title={`Welcome, ${user?.fullName||'Customer'}`} subtitle="Your account overview"/>
      <div className="vp-dash-grid">
        <DashboardCard title="My Vehicles"     value={data.vehicles.length}    accent="var(--accent)"/>
        <DashboardCard title="Total Purchases" value={data.purchases.length}   accent="#6366f1"/>
        <DashboardCard title="Appointments"    value={data.appointments.length} accent="var(--success)"/>
        <DashboardCard title="Part Predictions" value={predictions.length}     sub="Active alerts" accent="var(--warning)"/>
      </div>
      <div className="vp-dash-row">
        <Card className="vp-flex2">
          <div className="vp-section-title">My Vehicles</div>
          {data.vehicles.length===0?<EmptyState message="No vehicles registered."/>:(
            <div className="vp-vehicle-cards">
              {data.vehicles.map(v=>(
                <div key={v.id} className="vp-vehicle-item">
                  <div className="vp-vehicle-icon"><NavIcon name="car" size={18}/></div>
                  <div><div className="vp-vehicle-number">{v.vehicleNumber}</div><div className="vp-vehicle-desc">{v.brand} {v.model} — {v.vehicleType} ({v.year})</div></div>
                </div>
              ))}
            </div>
          )}
        </Card>
        <Card className="vp-flex1">
          <div className="vp-section-title">Quick Actions</div>
          <div className="vp-quick-actions">
            <a href="#/customer/appointments"  className="vp-quick-btn"><NavIcon name="calendar" size={15}/> Book Appointment</a>
            <a href="#/customer/request-part"  className="vp-quick-btn"><NavIcon name="tool"     size={15}/> Request Part</a>
            <a href="#/customer/history"       className="vp-quick-btn"><NavIcon name="history"  size={15}/> View History</a>
            <a href="#/customer/reviews"       className="vp-quick-btn"><NavIcon name="star"     size={15}/> Submit Review</a>
          </div>
        </Card>
      </div>
      {predictions.length>0&&(
        <Card>
          <div className="vp-section-title">Part Failure Predictions</div>
          <div className="vp-pred-mini-list">
            {predictions.map(p=>(
              <div key={p.id} className="vp-pred-mini">
                <StatusBadge status={p.riskLevel}/>
                <div className="vp-pred-mini-info"><strong style={{color:'var(--tx)'}}>{p.part}</strong> — {p.vehicle}<div className="vp-text-sm vp-tx3" style={{marginTop:2}}>{p.action}</div></div>
              </div>
            ))}
          </div>
          <a href="#/customer/predictions" className="vp-view-more">View all predictions →</a>
        </Card>
      )}
    </CustomerLayout>
  );
}

function CustomerProfile() {
  const user=auth.getUser();
  const [form,setForm]=useState({fullName:user?.fullName||'',email:'customer@gmail.com',phone:'9841111001',address:'Baneshwor, Kathmandu'});
  const [editing,setEditing]=useState(false); const [errors,setErrors]=useState({});
  const {show,Toast}=useToast();
  const set=k=>e=>setForm(f=>({...f,[k]:e.target.value}));
  const validate=()=>{const e={};if(!form.fullName)e.fullName='Required';if(!form.phone)e.phone='Required';if(form.email&&!/\S+@\S+\.\S+/.test(form.email))e.email='Invalid email';return e;};
  const handleSave=()=>{const e=validate();if(Object.keys(e).length){setErrors(e);return;}show('Profile updated');setEditing(false);};
  return (
    <CustomerLayout title="My Profile">
      {Toast}
      <PageHeader title="My Profile" subtitle="Manage your personal information" action={!editing&&<Button onClick={()=>setEditing(true)}>Edit Profile</Button>}/>
      <Card>
        <div className="vp-profile-header">
          <div className="vp-profile-avatar">{(form.fullName||'C')[0]}</div>
          <div><div className="vp-profile-name">{form.fullName}</div><div className="vp-profile-role">Customer</div></div>
        </div>
        {!editing?(
          <div className="vp-detail-grid">
            <div className="vp-detail-field"><span className="vp-detail-label">Full Name</span><span className="vp-detail-value">{form.fullName}</span></div>
            <div className="vp-detail-field"><span className="vp-detail-label">Email</span><span className="vp-detail-value">{form.email}</span></div>
            <div className="vp-detail-field"><span className="vp-detail-label">Phone</span><span className="vp-detail-value">{form.phone}</span></div>
            <div className="vp-detail-field"><span className="vp-detail-label">Address</span><span className="vp-detail-value">{form.address}</span></div>
          </div>
        ):(
          <div style={{marginTop:16}}>
            <FormRow><Input label="Full Name" value={form.fullName} onChange={set('fullName')} error={errors.fullName} required/><Input label="Phone" value={form.phone} onChange={set('phone')} error={errors.phone} required/></FormRow>
            <Input label="Email" type="email" value={form.email} onChange={set('email')} error={errors.email}/>
            <Input label="Address" value={form.address} onChange={set('address')}/>
            <div className="vp-form-actions"><Button onClick={handleSave}>Save Changes</Button><Button variant="ghost" onClick={()=>setEditing(false)}>Cancel</Button></div>
          </div>
        )}
      </Card>
    </CustomerLayout>
  );
}

function MyVehicles() {
  const [vehicles,setVehicles]=useState([]); const [loading,setLoading]=useState(true);
  const [modal,setModal]=useState(false); const [editing,setEditing]=useState(null); const [confirm,setConfirm]=useState(null);
  const {show,Toast}=useToast();
  const empty={vehicleNumber:'',vehicleType:'Car',brand:'',model:'',year:''};
  const [form,setForm]=useState(empty); const [errors,setErrors]=useState({});
  const set=k=>e=>setForm(f=>({...f,[k]:e.target.value}));
  useEffect(()=>{api.getVehicles(1).then(setVehicles).finally(()=>setLoading(false));}, []);
  const validate=()=>{const e={};if(!form.vehicleNumber)e.vehicleNumber='Required';if(!form.brand)e.brand='Required';return e;};
  const handleSave=()=>{const e=validate();if(Object.keys(e).length){setErrors(e);return;}if(editing){setVehicles(v=>v.map(x=>x.id===editing.id?{...x,...form}:x));show('Vehicle updated');}else{setVehicles(v=>[...v,{...form,id:Date.now(),customerId:1}]);show('Vehicle added');}setModal(false);};
  return (
    <CustomerLayout title="My Vehicles">
      {Toast}
      <PageHeader title="My Vehicles" subtitle={`${vehicles.length} vehicle(s) registered`} action={<Button onClick={()=>{setEditing(null);setForm(empty);setErrors({});setModal(true);}}>Add Vehicle</Button>}/>
      {loading?<Loader/>:(
        <div className="vp-vehicle-grid">
          {vehicles.length===0?<EmptyState message="No vehicles registered yet."/>:vehicles.map(v=>(
            <Card key={v.id} className="vp-vehicle-card">
              <div className="vp-vehicle-card-header">
                <div className="vp-vehicle-icon-lg"><NavIcon name="car" size={20}/></div>
                <Badge color="blue">{v.vehicleType}</Badge>
              </div>
              <div className="vp-vehicle-number-lg">{v.vehicleNumber}</div>
              <div className="vp-vehicle-spec">{v.brand} {v.model}</div>
              <div className="vp-vehicle-year">Year: {v.year}</div>
              <div className="vp-form-actions" style={{marginTop:14}}>
                <Button size="sm" variant="ghost" onClick={()=>{setEditing(v);setForm({...v});setErrors({});setModal(true);}}>Edit</Button>
                <Button size="sm" variant="danger" onClick={()=>setConfirm(v)}>Remove</Button>
              </div>
            </Card>
          ))}
        </div>
      )}
      <Modal open={modal} onClose={()=>setModal(false)} title={editing?'Edit Vehicle':'Add Vehicle'}
        footer={<div style={{display:'flex',gap:8,justifyContent:'flex-end'}}><Button variant="ghost" onClick={()=>setModal(false)}>Cancel</Button><Button onClick={handleSave}>Save</Button></div>}>
        <Input label="Vehicle Number" value={form.vehicleNumber} onChange={set('vehicleNumber')} error={errors.vehicleNumber} required placeholder="BA 1 PA 1234"/>
        <FormRow><Select label="Vehicle Type" value={form.vehicleType} onChange={set('vehicleType')}>{['Car','SUV','Truck','Van','Motorcycle','Other'].map(t=><option key={t}>{t}</option>)}</Select><Input label="Brand" value={form.brand} onChange={set('brand')} error={errors.brand} required placeholder="Toyota..."/></FormRow>
        <FormRow><Input label="Model" value={form.model} onChange={set('model')} placeholder="Vitz..."/><Input label="Year" type="number" value={form.year} onChange={set('year')} placeholder="2020" min="1990" max="2025"/></FormRow>
      </Modal>
      <ConfirmDialog open={!!confirm} onClose={()=>setConfirm(null)} title="Remove Vehicle" message={`Remove vehicle ${confirm?.vehicleNumber}?`} onConfirm={()=>{setVehicles(v=>v.filter(x=>x.id!==confirm.id));show('Vehicle removed');setConfirm(null);}}/>
    </CustomerLayout>
  );
}

function BookAppointment() {
  const [appointments,setAppointments]=useState([]); const [vehicles,setVehicles]=useState([]);
  const [loading,setLoading]=useState(true); const [view,setView]=useState('list');
  const [form,setForm]=useState({vehicleId:'',serviceType:'',date:'',time:'',description:''});
  const [errors,setErrors]=useState({});
  const {show,Toast}=useToast();
  const set=k=>e=>setForm(f=>({...f,[k]:e.target.value}));
  useEffect(()=>{Promise.all([api.getAppointments(),api.getVehicles(1)]).then(([a,v])=>{setAppointments(a.filter(x=>x.customerId===1));setVehicles(v);}).finally(()=>setLoading(false));}, []);
  const validate=()=>{const e={};if(!form.vehicleId)e.vehicleId='Select a vehicle';if(!form.serviceType)e.serviceType='Select service type';if(!form.date)e.date='Date is required';else if(new Date(form.date)<new Date())e.date='Date cannot be in the past';if(!form.time)e.time='Time is required';return e;};
  const handleBook=e=>{e.preventDefault();const errs=validate();if(Object.keys(errs).length){setErrors(errs);return;}const vehicle=vehicles.find(v=>v.id==form.vehicleId);setAppointments(prev=>[...prev,{id:Date.now(),customerId:1,vehicleNumber:vehicle?.vehicleNumber,serviceType:form.serviceType,date:form.date,time:form.time,description:form.description,status:'Pending'}]);show('Appointment booked successfully');setForm({vehicleId:'',serviceType:'',date:'',time:'',description:''});setView('list');};
  return (
    <CustomerLayout title="Appointments">
      {Toast}
      <PageHeader title="Appointments" subtitle="Book and manage your service appointments"
        action={<Button onClick={()=>setView(v=>v==='book'?'list':'book')}>{view==='book'?'Back to List':'Book Appointment'}</Button>}/>
      {view==='book'?(
        <Card>
          <div className="vp-section-title">New Appointment</div>
          <form onSubmit={handleBook}>
            <FormRow>
              <Select label="Vehicle" required value={form.vehicleId} onChange={set('vehicleId')} error={errors.vehicleId}><option value="">Select your vehicle</option>{vehicles.map(v=><option key={v.id} value={v.id}>{v.vehicleNumber} — {v.brand} {v.model}</option>)}</Select>
              <Select label="Service Type" required value={form.serviceType} onChange={set('serviceType')} error={errors.serviceType}><option value="">Select service</option>{['General Service','Oil Change','Brake Inspection','Tire Change','Battery Check','AC Service','Engine Diagnostic','Other'].map(s=><option key={s}>{s}</option>)}</Select>
            </FormRow>
            <FormRow>
              <Input label="Preferred Date" type="date" required value={form.date} onChange={set('date')} error={errors.date}/>
              <Input label="Preferred Time" type="time" required value={form.time} onChange={set('time')} error={errors.time}/>
            </FormRow>
            <Textarea label="Description" value={form.description} onChange={set('description')} placeholder="Describe the issue or service needed..."/>
            <div className="vp-form-actions"><Button type="submit">Book Appointment</Button><Button variant="ghost" type="button" onClick={()=>setView('list')}>Cancel</Button></div>
          </form>
        </Card>
      ):(
        <Card>{loading?<Loader/>:appointments.length===0?<EmptyState message="No appointments yet."/>:(
          <Table columns={[
            {label:'Service Type',key:'serviceType'},
            {label:'Vehicle',key:'vehicleNumber'},
            {label:'Date',render:a=>formatDate(a.date)},
            {label:'Time',key:'time'},
            {label:'Status',render:a=><StatusBadge status={a.status}/>},
            {label:'Description',render:a=>a.description||'—'},
          ]} data={appointments}/>
        )}</Card>
      )}
    </CustomerLayout>
  );
}

function RequestUnavailablePart() {
  const [requests,setRequests]=useState([]); const [vehicles,setVehicles]=useState([]);
  const [loading,setLoading]=useState(true); const [view,setView]=useState('list');
  const [form,setForm]=useState({partName:'',vehicleId:'',description:'',urgency:'Medium'});
  const [errors,setErrors]=useState({});
  const {show,Toast}=useToast();
  const set=k=>e=>setForm(f=>({...f,[k]:e.target.value}));
  useEffect(()=>{Promise.all([api.getPartRequests(),api.getVehicles(1)]).then(([r,v])=>{setRequests(r.filter(x=>x.customerId===1));setVehicles(v);}).finally(()=>setLoading(false));}, []);
  const validate=()=>{const e={};if(!form.partName)e.partName='Required';if(!form.description)e.description='Required';return e;};
  const handleSubmit=e=>{e.preventDefault();const errs=validate();if(Object.keys(errs).length){setErrors(errs);return;}const vehicle=vehicles.find(v=>v.id==form.vehicleId);setRequests(prev=>[...prev,{id:Date.now(),customerId:1,customerName:'Pramod Karki',partName:form.partName,vehicleNumber:vehicle?.vehicleNumber||'—',description:form.description,urgency:form.urgency,status:'Pending',date:new Date().toISOString().split('T')[0]}]);show('Part request submitted');setForm({partName:'',vehicleId:'',description:'',urgency:'Medium'});setView('list');};
  return (
    <CustomerLayout title="Request Part">
      {Toast}
      <PageHeader title="Request Unavailable Part" subtitle="Submit requests for parts not in stock"
        action={<Button onClick={()=>setView(v=>v==='form'?'list':'form')}>{view==='form'?'Back to List':'New Request'}</Button>}/>
      {view==='form'?(
        <Card>
          <form onSubmit={handleSubmit}>
            <Input label="Part Name" required value={form.partName} onChange={set('partName')} error={errors.partName} placeholder="e.g. Toyota Vitz Door Mirror Left"/>
            <Select label="Vehicle (Optional)" value={form.vehicleId} onChange={set('vehicleId')}><option value="">Select vehicle</option>{vehicles.map(v=><option key={v.id} value={v.id}>{v.vehicleNumber} — {v.brand} {v.model}</option>)}</Select>
            <Select label="Urgency" value={form.urgency} onChange={set('urgency')}><option>Low</option><option>Medium</option><option>High</option></Select>
            <Textarea label="Description" required value={form.description} onChange={set('description')} error={errors.description} placeholder="Describe the part in detail..."/>
            <div className="vp-form-actions"><Button type="submit">Submit Request</Button><Button variant="ghost" type="button" onClick={()=>setView('list')}>Cancel</Button></div>
          </form>
        </Card>
      ):(
        <Card>{loading?<Loader/>:requests.length===0?<EmptyState message="No part requests yet."/>:(
          <Table columns={[
            {label:'Part Name',key:'partName'},
            {label:'Vehicle',key:'vehicleNumber'},
            {label:'Urgency',render:r=><StatusBadge status={r.urgency}/>},
            {label:'Status',render:r=><StatusBadge status={r.status}/>},
            {label:'Date',render:r=>formatDate(r.date)},
          ]} data={requests}/>
        )}</Card>
      )}
    </CustomerLayout>
  );
}

function MyHistory() {
  const [tab,setTab]=useState('purchases');
  const [history,setHistory]=useState({purchases:[],appointments:[]});
  const [loading,setLoading]=useState(true);
  useEffect(()=>{api.getCustomerHistory(1).then(setHistory).finally(()=>setLoading(false));}, []);
  return (
    <CustomerLayout title="My History">
      <PageHeader title="My History" subtitle="Purchase and service records"/>
      <div className="vp-tab-bar">
        {[['purchases','Purchases'],['services','Service History'],['appointments','Appointments'],['credits','Credits']].map(([key,label])=>(
          <button key={key} className={`vp-tab-btn ${tab===key?'vp-tab-active':''}`} onClick={()=>setTab(key)}>{label}</button>
        ))}
      </div>
      {loading?<Loader/>:(
        <Card>
          {tab==='purchases'&&<Table columns={[{label:'Invoice #',key:'invoiceNumber'},{label:'Date',render:i=>formatDate(i.date)},{label:'Subtotal',render:i=>formatCurrency(i.subtotal)},{label:'Discount',render:i=>i.discount>0?<span className="vp-success-color">- {formatCurrency(i.discount)}</span>:'—'},{label:'Total',render:i=><span className="vp-fw6">{formatCurrency(i.totalAmount)}</span>},{label:'Status',render:i=><StatusBadge status={i.paymentStatus}/>}]} data={history.purchases} emptyMessage="No purchase history."/>}
          {tab==='services'&&<EmptyState message="No service history records yet."/>}
          {tab==='appointments'&&<Table columns={[{label:'Service',key:'serviceType'},{label:'Vehicle',key:'vehicleNumber'},{label:'Date',render:a=>formatDate(a.date)},{label:'Status',render:a=><StatusBadge status={a.status}/>}]} data={history.appointments} emptyMessage="No appointments."/>}
          {tab==='credits'&&<Table columns={[{label:'Invoice #',key:'invoiceNumber'},{label:'Date',render:i=>formatDate(i.date)},{label:'Amount',render:i=><span className="vp-error-color vp-fw6">{formatCurrency(i.totalAmount)}</span>},{label:'Due Date',render:i=>i.creditDueDate?formatDate(i.creditDueDate):'—'},{label:'Status',render:i=><StatusBadge status={i.paymentStatus}/>}]} data={history.purchases.filter(i=>i.paymentStatus==='Credit')} emptyMessage="No credit invoices."/>}
        </Card>
      )}
    </CustomerLayout>
  );
}

function SubmitReview() {
  const [reviews,setReviews]=useState([]); const [loading,setLoading]=useState(true);
  const [form,setForm]=useState({rating:0,comment:''}); const [errors,setErrors]=useState({});
  const {show,Toast}=useToast();
  const set=k=>e=>setForm(f=>({...f,[k]:e.target.value}));
  useEffect(()=>{api.getReviews().then(r=>setReviews(r.filter(x=>x.customerId===1))).finally(()=>setLoading(false));}, []);
  const validate=()=>{const e={};if(!form.rating||form.rating<1||form.rating>5)e.rating='Please select a rating (1–5)';if(!form.comment.trim())e.comment='Comment is required';return e;};
  const handleSubmit=e=>{e.preventDefault();const errs=validate();if(Object.keys(errs).length){setErrors(errs);return;}setReviews(prev=>[...prev,{id:Date.now(),customerId:1,customerName:'Pramod Karki',rating:form.rating,comment:form.comment,date:new Date().toISOString().split('T')[0]}]);show('Review submitted');setForm({rating:0,comment:''});setErrors({});};
  return (
    <CustomerLayout title="Reviews">
      {Toast}
      <PageHeader title="Reviews" subtitle="Share your experience"/>
      <Card>
        <div className="vp-section-title">Submit a Review</div>
        <form onSubmit={handleSubmit}>
          <div className="vp-form-group">
            <label className="vp-label">Rating <span className="vp-required">*</span></label>
            <StarRating value={form.rating} onChange={v=>setForm(f=>({...f,rating:v}))}/>
            {errors.rating&&<p className="vp-field-error">{errors.rating}</p>}
          </div>
          <Textarea label="Your Comment" required value={form.comment} onChange={set('comment')} error={errors.comment} placeholder="Tell us about your experience..." rows={4}/>
          <Button type="submit">Submit Review</Button>
        </form>
      </Card>
      <Card style={{marginTop:16}}>
        <div className="vp-section-title">My Reviews</div>
        {loading?<Loader/>:reviews.length===0?<EmptyState message="No reviews yet."/>:(
          <div className="vp-reviews-list">
            {reviews.map(r=>(
              <div key={r.id} className="vp-review-item">
                <div className="vp-review-header">
                  <StarRating value={r.rating}/>
                  <span className="vp-review-date">{formatDate(r.date)}</span>
                </div>
                <p className="vp-review-comment">{r.comment}</p>
              </div>
            ))}
          </div>
        )}
      </Card>
    </CustomerLayout>
  );
}

function PartFailurePrediction() {
  const [predictions,setPredictions]=useState([]); const [loading,setLoading]=useState(true);
  useEffect(()=>{api.getPredictions(1).then(setPredictions).finally(()=>setLoading(false));}, []);
  const riskBorder={High:'var(--error)',Medium:'var(--warning)',Low:'var(--success)'};
  return (
    <CustomerLayout title="Predictions">
      <PageHeader title="Part Failure Predictions" subtitle="AI-based analysis of your vehicle components"/>
      <Alert type="info" message="Predictions are based on service history, purchase records, and usage patterns. Always consult a mechanic for a final assessment."/>
      {loading?<Loader text="Analysing vehicle data..."/>:(
        <div style={{marginTop:16}}>
          {predictions.length===0?<EmptyState message="No predictions available for your vehicles."/>:predictions.map(p=>(
            <Card key={p.id} className="vp-pred-card">
              <div className="vp-pred-header">
                <div><div className="vp-pred-vehicle">{p.vehicle}</div><div className="vp-pred-vnumber">{p.vehicleNumber}</div></div>
                <StatusBadge status={p.riskLevel}/>
              </div>
              <div className="vp-pred-row"><span className="vp-pred-lbl">Part at Risk</span><span style={{color:'var(--tx)',fontWeight:600}}>{p.part}</span></div>
              <div className="vp-pred-row"><span className="vp-pred-lbl">Reason</span><span>{p.reason}</span></div>
              <div className="vp-pred-action" style={{borderLeftColor:riskBorder[p.riskLevel]||'var(--accent)'}}><strong style={{color:'var(--tx)'}}>Suggested Action:</strong> {p.action}</div>
            </Card>
          ))}
        </div>
      )}
    </CustomerLayout>
  );
}

export { CustomerDashboard, CustomerProfile, MyVehicles, BookAppointment, RequestUnavailablePart, MyHistory, SubmitReview, PartFailurePrediction  };
