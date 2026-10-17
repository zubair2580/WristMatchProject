import { CheckCircle2, Plus, Search, XCircle } from 'lucide-react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState, type FormEvent } from 'react'
import AdminModal from '../components/AdminModal'
import { adminMutation } from '../store/adminApi'
import { apiUrl } from '../store/api'

type OrderListItem = { id: string; order: string; customer: string; placed: string; total: number; status: string; payment: string; fulfilment: string; currency: string }
type OrderDetail = { id: string; orderNumber: string; customerId: string; customer: string; warehouseId: string; warehouse: string; status: string; paymentStatus: string; fulfilmentStatus: string; currency: string; taxRate: number; subtotal: number; taxTotal: number; grandTotal: number; notes?: string; lines: { id: string; productId: string; product: string; quantity: number; unitPrice: number; taxAmount: number; lineTotal: number }[] }
type LineDraft = { productId: string; quantity: string }

export default function AdminOrdersPage() {
  const client = useQueryClient()
  const [search, setSearch] = useState('')
  const [creating, setCreating] = useState(false)
  const [detailId, setDetailId] = useState<string | null>(null)
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)
  const [form, setForm] = useState({ customerId: '', warehouseId: '', notes: '', lines: [{ productId: '', quantity: '1' }] as LineDraft[] })

  const list = useQuery<{ items: OrderListItem[]; total: number }>({
    queryKey: ['admin-orders', search],
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/admin/orders?search=${encodeURIComponent(search)}&page=1&pageSize=50`, { credentials: 'include' })
      if (!response.ok) throw new Error('Unable to load orders')
      return response.json()
    },
  })
  const detail = useQuery<OrderDetail>({
    queryKey: ['admin-order', detailId],
    enabled: Boolean(detailId),
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/admin/orders/${detailId}`, { credentials: 'include' })
      if (!response.ok) throw new Error('Unable to load order')
      return response.json()
    },
  })
  const customers = useQuery<{ items: { id: string; firstName: string; lastName: string; isActive: boolean }[] }>({
    queryKey: ['customers', '', 1],
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/admin/customers?page=1&pageSize=100`, { credentials: 'include' })
      if (!response.ok) throw new Error('Unable to load customers')
      return response.json()
    },
  })
  const warehouses = useQuery<{ items: { id: string; code: string; name: string; isActive: boolean }[] }>({
    queryKey: ['warehouses-options'],
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/admin/warehouses?page=1&pageSize=100`, { credentials: 'include' })
      if (!response.ok) throw new Error('Unable to load warehouses')
      return response.json()
    },
  })
  const products = useQuery<{ id: string; name: string; price: number; currency: string; isActive: boolean }[]>({
    queryKey: ['admin-products'],
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/admin/products`, { credentials: 'include' })
      if (!response.ok) throw new Error('Unable to load products')
      return response.json()
    },
  })

  const customerOptions = useMemo(() => (customers.data?.items ?? []).filter(item => item.isActive !== false).map(item => ({ id: item.id, label: `${item.firstName} ${item.lastName}` })), [customers.data])
  const warehouseOptions = useMemo(() => (warehouses.data?.items ?? []).filter(item => item.isActive).map(item => ({ id: item.id, label: `${item.code} · ${item.name}` })), [warehouses.data])
  const productOptions = useMemo(() => (products.data ?? []).filter(item => item.isActive).map(item => ({ id: item.id, label: `${item.name} · ${item.currency} ${item.price.toLocaleString()}`, price: item.price })), [products.data])

  const openCreate = () => {
    setCreating(true)
    setError('')
    setForm({
      customerId: customerOptions[0]?.id ?? '',
      warehouseId: warehouseOptions[0]?.id ?? '',
      notes: '',
      lines: [{ productId: productOptions[0]?.id ?? '', quantity: '1' }],
    })
  }

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setSaving(true)
    setError('')
    try {
      const created = await adminMutation<OrderDetail>('/admin/orders', 'POST', JSON.stringify({
        customerId: form.customerId,
        warehouseId: form.warehouseId,
        currency: 'PKR',
        notes: form.notes,
        lines: form.lines.map(line => ({ productId: line.productId, quantity: Number(line.quantity) })),
      }), { 'Content-Type': 'application/json' })
      await client.invalidateQueries({ queryKey: ['admin-orders'] })
      setCreating(false)
      setDetailId(created.id)
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Unable to create order.')
    } finally {
      setSaving(false)
    }
  }

  const transition = async (action: string) => {
    if (!detailId) return
    setSaving(true)
    setError('')
    try {
      await adminMutation(`/admin/orders/${detailId}/transitions`, 'POST', JSON.stringify({ action }), { 'Content-Type': 'application/json' })
      await client.invalidateQueries({ queryKey: ['admin-order', detailId] })
      await client.invalidateQueries({ queryKey: ['admin-orders'] })
      await client.invalidateQueries({ queryKey: ['admin-stock'] })
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Unable to update order.')
    } finally {
      setSaving(false)
    }
  }

  const order = detail.data
  const actions = order ? availableActions(order.status, order.paymentStatus) : []

  return (
    <div>
      <div className="page-heading">
        <div>
          <p className="eyebrow">SALES OPERATIONS</p>
          <h1>Orders</h1>
          <p>Create draft orders, confirm to reserve stock, then process and complete fulfilment.</p>
        </div>
        <button className="primary-button compact-button" onClick={openCreate}><Plus size={17} /> Create order</button>
      </div>
      <section className="panel table-panel">
        <div className="table-toolbar">
          <div className="search-box"><Search size={17} /><input value={search} onChange={event => setSearch(event.target.value)} placeholder="Search order, customer or status" /></div>
        </div>
        <div className="table-scroll">
          <table>
            <thead>
              <tr><th>Sr #</th><th>Order</th><th>Customer</th><th>Placed</th><th>Total</th><th>Status</th><th>Payment</th><th>Fulfilment</th></tr>
            </thead>
            <tbody>
              {(list.data?.items ?? []).map((item, index) => (
                <tr key={item.id} className="clickable-row" onClick={() => { setDetailId(item.id); setError('') }}>
                  <td>{index + 1}</td>
                  <td><strong>{item.order}</strong></td>
                  <td>{item.customer}</td>
                  <td>{new Date(item.placed).toLocaleDateString()}</td>
                  <td>{item.currency} {Number(item.total).toLocaleString()}</td>
                  <td><span className="status-pill"><i /> {item.status}</span></td>
                  <td>{item.payment}</td>
                  <td>{item.fulfilment}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {list.isPending && <div className="module-empty"><h2>Loading orders...</h2></div>}
        {!list.isPending && (list.data?.items.length ?? 0) === 0 && <div className="module-empty"><div className="module-empty-mark">W</div><h2>No orders yet</h2><p>Create a draft order to start the sales workflow.</p></div>}
        <div className="table-pagination"><span>{list.data?.total ?? 0} orders</span></div>
      </section>

      {creating && (
        <AdminModal
          eyebrow="SALES ORDER"
          title="Create order"
          onClose={() => setCreating(false)}
          footer={<><button className="outline-button" onClick={() => setCreating(false)}>Cancel</button><button className="primary-button compact-button" form="order-form" disabled={saving}>{saving ? 'Creating...' : 'Create draft'}</button></>}
        >
          <form id="order-form" className="admin-form" onSubmit={submit}>
            <div className="form-grid">
              <label>Customer
                <select required value={form.customerId} onChange={event => setForm(current => ({ ...current, customerId: event.target.value }))}>
                  {customerOptions.map(item => <option key={item.id} value={item.id}>{item.label}</option>)}
                </select>
              </label>
              <label>Warehouse
                <select required value={form.warehouseId} onChange={event => setForm(current => ({ ...current, warehouseId: event.target.value }))}>
                  {warehouseOptions.map(item => <option key={item.id} value={item.id}>{item.label}</option>)}
                </select>
              </label>
              <label className="form-span">Notes<textarea rows={2} value={form.notes} onChange={event => setForm(current => ({ ...current, notes: event.target.value }))} /></label>
            </div>
            <div className="order-lines">
              <div className="order-lines-head"><strong>Lines</strong><button type="button" className="outline-button" onClick={() => setForm(current => ({ ...current, lines: [...current.lines, { productId: productOptions[0]?.id ?? '', quantity: '1' }] }))}>Add line</button></div>
              {form.lines.map((line, index) => (
                <div className="order-line-row" key={index}>
                  <select required value={line.productId} onChange={event => setForm(current => ({ ...current, lines: current.lines.map((item, itemIndex) => itemIndex === index ? { ...item, productId: event.target.value } : item) }))}>
                    {productOptions.map(item => <option key={item.id} value={item.id}>{item.label}</option>)}
                  </select>
                  <input required min="0.0001" step="0.0001" type="number" value={line.quantity} onChange={event => setForm(current => ({ ...current, lines: current.lines.map((item, itemIndex) => itemIndex === index ? { ...item, quantity: event.target.value } : item) }))} />
                  {form.lines.length > 1 && <button type="button" className="row-action danger" onClick={() => setForm(current => ({ ...current, lines: current.lines.filter((_, itemIndex) => itemIndex !== index) }))}><XCircle size={16} /></button>}
                </div>
              ))}
            </div>
            {error && <div className="form-error">{error}</div>}
          </form>
        </AdminModal>
      )}

      {detailId && order && (
        <AdminModal
          eyebrow="ORDER DETAIL"
          title={order.orderNumber}
          onClose={() => setDetailId(null)}
          footer={
            <>
              <button className="outline-button" onClick={() => setDetailId(null)}>Close</button>
              {actions.map(action => (
                <button key={action.key} className="primary-button compact-button" disabled={saving} onClick={() => transition(action.key)}>
                  <CheckCircle2 size={16} /> {action.label}
                </button>
              ))}
            </>
          }
        >
          <div className="order-detail">
            <div className="order-meta">
              <div><span>Customer</span><strong>{order.customer}</strong></div>
              <div><span>Warehouse</span><strong>{order.warehouse}</strong></div>
              <div><span>Status</span><strong>{order.status}</strong></div>
              <div><span>Payment</span><strong>{order.paymentStatus}</strong></div>
              <div><span>Fulfilment</span><strong>{order.fulfilmentStatus}</strong></div>
              <div><span>Total</span><strong>{order.currency} {order.grandTotal.toLocaleString()}</strong></div>
            </div>
            <table>
              <thead><tr><th>Product</th><th>Qty</th><th>Unit</th><th>Tax</th><th>Line</th></tr></thead>
              <tbody>
                {order.lines.map(line => (
                  <tr key={line.id}>
                    <td>{line.product}</td>
                    <td>{line.quantity}</td>
                    <td>{line.unitPrice.toLocaleString()}</td>
                    <td>{line.taxAmount.toLocaleString()}</td>
                    <td>{line.lineTotal.toLocaleString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            <div className="order-totals">
              <span>Subtotal {order.subtotal.toLocaleString()}</span>
              <span>Tax ({Math.round(order.taxRate * 100)}%) {order.taxTotal.toLocaleString()}</span>
              <strong>Grand {order.grandTotal.toLocaleString()}</strong>
            </div>
            {error && <div className="form-error">{error}</div>}
          </div>
        </AdminModal>
      )}
    </div>
  )
}

function availableActions(status: string, paymentStatus: string) {
  const actions: { key: string; label: string }[] = []
  if (status === 'Draft') actions.push({ key: 'confirm', label: 'Confirm & reserve' })
  if (status === 'Confirmed') actions.push({ key: 'process', label: 'Start processing' }, { key: 'complete', label: 'Complete' })
  if (status === 'Processing') actions.push({ key: 'complete', label: 'Complete' })
  if (status !== 'Completed' && status !== 'Cancelled') actions.push({ key: 'cancel', label: 'Cancel' })
  if (paymentStatus === 'Pending' && status !== 'Cancelled') actions.push({ key: 'mark-paid', label: 'Mark paid' })
  return actions
}
