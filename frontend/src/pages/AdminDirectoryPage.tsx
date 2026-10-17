import { Archive, ChevronLeft, ChevronRight, Pencil, Plus, Search } from 'lucide-react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useState, type FormEvent } from 'react'
import AdminModal from '../components/AdminModal'
import { adminMutation } from '../store/adminApi'
import { apiUrl } from '../store/api'

type Mode = 'customers' | 'suppliers'
type FormState = Record<string, string | number | boolean>

export default function AdminDirectoryPage({ mode }: { mode: Mode }) {
  const customer = mode === 'customers'
  const title = customer ? 'Customers' : 'Suppliers'
  const eyebrow = customer ? 'CUSTOMER RELATIONSHIPS' : 'SUPPLY CHAIN'
  const endpoint = customer ? 'customers' : 'suppliers'
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [editing, setEditing] = useState<{ id: string; rowVersion: string; [key: string]: unknown } | null | undefined>(undefined)
  const [form, setForm] = useState<FormState>(customer
    ? { firstName: '', lastName: '', email: '', phone: '', marketingConsent: false, isGuest: true, isActive: true, rowVersion: '' }
    : { name: '', email: '', phone: '', currency: 'PKR', paymentTermsDays: 30, isActive: true, rowVersion: '' })
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)
  const client = useQueryClient()
  const pageSize = 10
  const query = useQuery<{ items: { id: string; rowVersion: string; [key: string]: unknown }[]; page: number; total: number; totalPages: number }>({
    queryKey: [endpoint, search, page],
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/admin/${endpoint}?search=${encodeURIComponent(search)}&page=${page}&pageSize=${pageSize}`, { credentials: 'include' })
      if (!response.ok) throw new Error(`Unable to load ${title.toLowerCase()}`)
      return response.json()
    },
  })
  const openCreate = () => {
    setEditing(null)
    setForm(customer
      ? { firstName: '', lastName: '', email: '', phone: '', marketingConsent: false, isGuest: true, isActive: true, rowVersion: '' }
      : { name: '', email: '', phone: '', currency: 'PKR', paymentTermsDays: 30, isActive: true, rowVersion: '' })
    setError('')
  }
  const openEdit = (item: { id: string; rowVersion: string; [key: string]: unknown }) => {
    setEditing(item)
    const fields = customer
      ? { firstName: item.firstName, lastName: item.lastName, email: item.email, phone: item.phone, marketingConsent: item.marketingConsent, isGuest: item.isGuest, isActive: item.isActive ?? true, rowVersion: item.rowVersion }
      : { name: item.name, email: item.email, phone: item.phone, currency: item.currency, paymentTermsDays: item.paymentTermsDays, isActive: item.isActive, rowVersion: item.rowVersion }
    setForm(fields as unknown as FormState)
    setError('')
  }
  const close = () => setEditing(undefined)
  const set = (key: string, value: string | number | boolean) => setForm(current => ({ ...current, [key]: value }))
  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setSaving(true)
    setError('')
    try {
      await adminMutation(`${editing ? `/admin/${endpoint}/${editing.id}` : `/admin/${endpoint}`}`, editing ? 'PUT' : 'POST', JSON.stringify(form), { 'Content-Type': 'application/json' })
      await client.invalidateQueries({ queryKey: [endpoint] })
      close()
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Unable to save record.')
    } finally {
      setSaving(false)
    }
  }
  const archive = async (item: { id: string; name?: unknown; firstName?: unknown }) => {
    if (!window.confirm(`Deactivate ${String(item.name ?? item.firstName)}?`)) return
    await adminMutation(`/admin/${endpoint}/${item.id}`, 'DELETE')
    await client.invalidateQueries({ queryKey: [endpoint] })
  }
  return (
    <div>
      <div className="page-heading">
        <div>
          <p className="eyebrow">{eyebrow}</p>
          <h1>{title}</h1>
          <p>{customer ? 'Manage guest and registered customer profiles.' : 'Maintain supplier contacts, terms, and purchasing relationships.'}</p>
        </div>
        <button className="primary-button compact-button" onClick={openCreate}><Plus size={17} /> Add {customer ? 'customer' : 'supplier'}</button>
      </div>
      <section className="panel table-panel">
        <div className="table-toolbar">
          <div className="search-box"><Search size={17} /><input value={search} onChange={event => { setSearch(event.target.value); setPage(1) }} placeholder={`Search ${title.toLowerCase()}`} /></div>
        </div>
        {query.isPending && <div className="empty-table">Loading {title.toLowerCase()}...</div>}
        {query.isError && <div className="admin-table-error">Unable to load {title.toLowerCase()}.</div>}
        {query.data && (
          <>
            <div className="table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>Sr #</th>
                    {customer ? <><th>Customer</th><th>Contact</th><th>Type</th><th>Consent</th><th>Status</th><th /></> : <><th>Supplier</th><th>Contact</th><th>Currency</th><th>Terms</th><th>Status</th><th /></>}
                  </tr>
                </thead>
                <tbody>
                  {query.data.items.map((item, index) => customer ? (
                    <tr key={item.id}>
                      <td>{(page - 1) * pageSize + index + 1}</td>
                      <td><strong>{String(item.firstName)} {String(item.lastName)}</strong><small>{String(item.customerNumber ?? item.id).slice(0, 12)}</small></td>
                      <td>{String(item.email)}<small>{String(item.phone)}</small></td>
                      <td>{item.isGuest ? 'Guest' : 'Account'}</td>
                      <td>{item.marketingConsent ? 'Opted in' : 'Private'}</td>
                      <td><span className={item.isActive ? 'status-pill' : 'status-pill archived'}><i /> {item.isActive ? 'Active' : 'Inactive'}</span></td>
                      <td>
                        <button className="row-action" onClick={() => openEdit(item)}><Pencil size={16} /></button>
                        {item.isActive !== false && <button className="row-action danger" onClick={() => archive(item)}><Archive size={16} /></button>}
                      </td>
                    </tr>
                  ) : (
                    <tr key={item.id}>
                      <td>{(page - 1) * pageSize + index + 1}</td>
                      <td><strong>{String(item.name)}</strong><small>{String(item.supplierNumber ?? item.id).slice(0, 12)}</small></td>
                      <td>{String(item.email)}<small>{String(item.phone)}</small></td>
                      <td>{String(item.currency)}</td>
                      <td>{String(item.paymentTermsDays)} days</td>
                      <td><span className={item.isActive ? 'status-pill' : 'status-pill archived'}><i /> {item.isActive ? 'Active' : 'Archived'}</span></td>
                      <td>
                        <button className="row-action" onClick={() => openEdit(item)}><Pencil size={16} /></button>
                        <button className="row-action danger" onClick={() => archive(item)}><Archive size={16} /></button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            {query.data.items.length === 0 && <div className="module-empty"><h2>No matching records</h2><p>Try another search or add a new {customer ? 'customer' : 'supplier'}.</p></div>}
            <div className="table-pagination">
              <span>{query.data.total} records</span>
              <div>
                <button disabled={page === 1} onClick={() => setPage(value => value - 1)}><ChevronLeft size={14} /> Previous</button>
                <span>Page {query.data.page} of {query.data.totalPages}</span>
                <button disabled={page === query.data.totalPages} onClick={() => setPage(value => value + 1)}>Next <ChevronRight size={14} /></button>
              </div>
            </div>
          </>
        )}
      </section>
      {editing !== undefined && (
        <AdminModal
          eyebrow={eyebrow}
          title={`${editing ? 'Edit' : 'Add'} ${customer ? 'customer' : 'supplier'}`}
          onClose={close}
          footer={<><button className="outline-button" onClick={close}>Cancel</button><button className="primary-button compact-button" form="directory-form" disabled={saving}>{saving ? 'Saving...' : 'Save record'}</button></>}
        >
          <form id="directory-form" className="admin-form" onSubmit={submit}>
            <div className="form-grid">
              {customer ? (
                <>
                  <label>First name<input required value={String(form.firstName)} onChange={event => set('firstName', event.target.value)} /></label>
                  <label>Last name<input required value={String(form.lastName)} onChange={event => set('lastName', event.target.value)} /></label>
                  <label>Email<input required type="email" value={String(form.email)} onChange={event => set('email', event.target.value)} /></label>
                  <label>Phone<input required value={String(form.phone)} onChange={event => set('phone', event.target.value)} /></label>
                  <label className="form-check"><input type="checkbox" checked={Boolean(form.isGuest)} onChange={event => set('isGuest', event.target.checked)} /> Guest profile</label>
                  <label className="form-check"><input type="checkbox" checked={Boolean(form.marketingConsent)} onChange={event => set('marketingConsent', event.target.checked)} /> Marketing consent</label>
                  <label className="form-check"><input type="checkbox" checked={Boolean(form.isActive)} onChange={event => set('isActive', event.target.checked)} /> Active customer</label>
                </>
              ) : (
                <>
                  <label>Supplier name<input required value={String(form.name)} onChange={event => set('name', event.target.value)} /></label>
                  <label>Email<input required type="email" value={String(form.email)} onChange={event => set('email', event.target.value)} /></label>
                  <label>Phone<input required value={String(form.phone)} onChange={event => set('phone', event.target.value)} /></label>
                  <label>Currency<input required maxLength={3} value={String(form.currency)} onChange={event => set('currency', event.target.value.toUpperCase())} /></label>
                  <label>Payment terms (days)<input type="number" min="0" value={Number(form.paymentTermsDays)} onChange={event => set('paymentTermsDays', Number(event.target.value))} /></label>
                  <label className="form-check"><input type="checkbox" checked={Boolean(form.isActive)} onChange={event => set('isActive', event.target.checked)} /> Active supplier</label>
                </>
              )}
            </div>
            {error && <div className="form-error">{error}</div>}
          </form>
        </AdminModal>
      )}
    </div>
  )
}
