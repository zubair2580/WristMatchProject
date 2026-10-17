import { CheckCircle2, Plus, Search, XCircle } from 'lucide-react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState, type FormEvent } from 'react'
import AdminModal from '../components/AdminModal'
import { adminMutation } from '../store/adminApi'
import { apiUrl } from '../store/api'

type PurchaseListItem = { id: string; purchaseOrder: string; supplier: string; warehouse: string; ordered: string; expected?: string | null; total: number; status: string; currency: string }
type PurchaseDetail = {
  id: string; purchaseOrderNumber: string; supplierId: string; supplier: string; warehouseId: string; warehouse: string; status: string
  currency: string; taxRate: number; subtotal: number; taxTotal: number; grandTotal: number; notes?: string
  lines: { id: string; productId: string; product: string; quantity: number; receivedQuantity: number; outstandingQuantity: number; unitCost: number; lineTotal: number }[]
}
type LineDraft = { productId: string; quantity: string; unitCost: string }

export default function AdminPurchasesPage() {
  const client = useQueryClient()
  const [search, setSearch] = useState('')
  const [creating, setCreating] = useState(false)
  const [detailId, setDetailId] = useState<string | null>(null)
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)
  const [receiveQty, setReceiveQty] = useState<Record<string, string>>({})
  const [form, setForm] = useState({ supplierId: '', warehouseId: '', notes: '', expectedAt: '', lines: [{ productId: '', quantity: '10', unitCost: '' }] as LineDraft[] })

  const list = useQuery<{ items: PurchaseListItem[]; total: number }>({
    queryKey: ['admin-purchases', search],
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/admin/purchases?search=${encodeURIComponent(search)}&page=1&pageSize=50`, { credentials: 'include' })
      if (!response.ok) throw new Error('Unable to load purchases')
      return response.json()
    },
  })
  const detail = useQuery<PurchaseDetail>({
    queryKey: ['admin-purchase', detailId],
    enabled: Boolean(detailId),
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/admin/purchases/${detailId}`, { credentials: 'include' })
      if (!response.ok) throw new Error('Unable to load purchase order')
      return response.json()
    },
  })
  const suppliers = useQuery<{ items: { id: string; name: string; isActive: boolean }[] }>({
    queryKey: ['suppliers', '', 1],
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/admin/suppliers?page=1&pageSize=100`, { credentials: 'include' })
      if (!response.ok) throw new Error('Unable to load suppliers')
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
  const products = useQuery<{ id: string; name: string; price: number; isActive: boolean }[]>({
    queryKey: ['admin-products'],
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/admin/products`, { credentials: 'include' })
      if (!response.ok) throw new Error('Unable to load products')
      return response.json()
    },
  })

  const supplierOptions = useMemo(() => (suppliers.data?.items ?? []).filter(item => item.isActive).map(item => ({ id: item.id, label: item.name })), [suppliers.data])
  const warehouseOptions = useMemo(() => (warehouses.data?.items ?? []).filter(item => item.isActive).map(item => ({ id: item.id, label: `${item.code} · ${item.name}` })), [warehouses.data])
  const productOptions = useMemo(() => (products.data ?? []).filter(item => item.isActive).map(item => ({ id: item.id, label: item.name, price: item.price })), [products.data])

  const openCreate = () => {
    setCreating(true)
    setError('')
    setForm({
      supplierId: supplierOptions[0]?.id ?? '',
      warehouseId: warehouseOptions[0]?.id ?? '',
      notes: '',
      expectedAt: '',
      lines: [{ productId: productOptions[0]?.id ?? '', quantity: '10', unitCost: productOptions[0] ? String(Math.round(productOptions[0].price * 0.6)) : '' }],
    })
  }

  const openDetail = (id: string) => {
    setDetailId(id)
    setError('')
    setReceiveQty({})
  }

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setSaving(true)
    setError('')
    try {
      const created = await adminMutation<PurchaseDetail>('/admin/purchases', 'POST', JSON.stringify({
        supplierId: form.supplierId,
        warehouseId: form.warehouseId,
        currency: 'PKR',
        notes: form.notes,
        expectedAt: form.expectedAt || null,
        lines: form.lines.map(line => ({ productId: line.productId, quantity: Number(line.quantity), unitCost: line.unitCost ? Number(line.unitCost) : null })),
      }), { 'Content-Type': 'application/json' })
      await client.invalidateQueries({ queryKey: ['admin-purchases'] })
      setCreating(false)
      openDetail(created.id)
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Unable to create purchase order.')
    } finally {
      setSaving(false)
    }
  }

  const transition = async (action: string) => {
    if (!detailId || !detail.data) return
    setSaving(true)
    setError('')
    try {
      const body: { action: string; lines?: { lineId: string; quantity: number }[] } = { action }
      if (action === 'receive') {
        const lines = detail.data.lines
          .filter(line => line.outstandingQuantity > 0)
          .map(line => ({
            lineId: line.id,
            quantity: Number(receiveQty[line.id] || line.outstandingQuantity),
          }))
          .filter(line => line.quantity > 0)
        body.lines = lines
      }
      await adminMutation(`/admin/purchases/${detailId}/transitions`, 'POST', JSON.stringify(body), { 'Content-Type': 'application/json' })
      await client.invalidateQueries({ queryKey: ['admin-purchase', detailId] })
      await client.invalidateQueries({ queryKey: ['admin-purchases'] })
      await client.invalidateQueries({ queryKey: ['admin-stock'] })
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Unable to update purchase order.')
    } finally {
      setSaving(false)
    }
  }

  const order = detail.data
  const actions = order ? availableActions(order.status) : []

  return (
    <div>
      <div className="page-heading">
        <div>
          <p className="eyebrow">PROCUREMENT</p>
          <h1>Purchases</h1>
          <p>Create purchase orders, approve spend, and receive goods into warehouse stock at weighted-average cost.</p>
        </div>
        <button className="primary-button compact-button" onClick={openCreate}><Plus size={17} /> New purchase order</button>
      </div>
      <section className="panel table-panel">
        <div className="table-toolbar">
          <div className="search-box"><Search size={17} /><input value={search} onChange={event => setSearch(event.target.value)} placeholder="Search PO, supplier, warehouse or status" /></div>
        </div>
        <div className="table-scroll">
          <table>
            <thead>
              <tr><th>Sr #</th><th>Purchase order</th><th>Supplier</th><th>Warehouse</th><th>Ordered</th><th>Expected</th><th>Total</th><th>Status</th></tr>
            </thead>
            <tbody>
              {(list.data?.items ?? []).map((item, index) => (
                <tr key={item.id} className="clickable-row" onClick={() => openDetail(item.id)}>
                  <td>{index + 1}</td>
                  <td><strong>{item.purchaseOrder}</strong></td>
                  <td>{item.supplier}</td>
                  <td>{item.warehouse}</td>
                  <td>{new Date(item.ordered).toLocaleDateString()}</td>
                  <td>{item.expected ? new Date(item.expected).toLocaleDateString() : '—'}</td>
                  <td>{item.currency} {Number(item.total).toLocaleString()}</td>
                  <td><span className="status-pill"><i /> {item.status}</span></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {list.isPending && <div className="module-empty"><h2>Loading purchases...</h2></div>}
        {!list.isPending && (list.data?.items.length ?? 0) === 0 && <div className="module-empty"><div className="module-empty-mark">W</div><h2>No purchase orders yet</h2><p>Create a draft PO to start procurement.</p></div>}
        <div className="table-pagination"><span>{list.data?.total ?? 0} purchase orders</span></div>
      </section>

      {creating && (
        <AdminModal
          eyebrow="PURCHASE ORDER"
          title="Create purchase order"
          onClose={() => setCreating(false)}
          footer={<><button className="outline-button" onClick={() => setCreating(false)}>Cancel</button><button className="primary-button compact-button" form="purchase-form" disabled={saving}>{saving ? 'Creating...' : 'Create draft'}</button></>}
        >
          <form id="purchase-form" className="admin-form" onSubmit={submit}>
            <div className="form-grid">
              <label>Supplier
                <select required value={form.supplierId} onChange={event => setForm(current => ({ ...current, supplierId: event.target.value }))}>
                  {supplierOptions.map(item => <option key={item.id} value={item.id}>{item.label}</option>)}
                </select>
              </label>
              <label>Warehouse
                <select required value={form.warehouseId} onChange={event => setForm(current => ({ ...current, warehouseId: event.target.value }))}>
                  {warehouseOptions.map(item => <option key={item.id} value={item.id}>{item.label}</option>)}
                </select>
              </label>
              <label>Expected date<input type="date" value={form.expectedAt} onChange={event => setForm(current => ({ ...current, expectedAt: event.target.value }))} /></label>
              <label className="form-span">Notes<textarea rows={2} value={form.notes} onChange={event => setForm(current => ({ ...current, notes: event.target.value }))} /></label>
            </div>
            <div className="order-lines">
              <div className="order-lines-head"><strong>Lines</strong><button type="button" className="outline-button" onClick={() => setForm(current => ({ ...current, lines: [...current.lines, { productId: productOptions[0]?.id ?? '', quantity: '10', unitCost: '' }] }))}>Add line</button></div>
              {form.lines.map((line, index) => (
                <div className="order-line-row purchase-line-row" key={index}>
                  <select required value={line.productId} onChange={event => setForm(current => ({ ...current, lines: current.lines.map((item, itemIndex) => itemIndex === index ? { ...item, productId: event.target.value } : item) }))}>
                    {productOptions.map(item => <option key={item.id} value={item.id}>{item.label}</option>)}
                  </select>
                  <input required min="0.0001" step="0.0001" type="number" value={line.quantity} onChange={event => setForm(current => ({ ...current, lines: current.lines.map((item, itemIndex) => itemIndex === index ? { ...item, quantity: event.target.value } : item) }))} />
                  <input required min="0" step="0.01" type="number" placeholder="Unit cost" value={line.unitCost} onChange={event => setForm(current => ({ ...current, lines: current.lines.map((item, itemIndex) => itemIndex === index ? { ...item, unitCost: event.target.value } : item) }))} />
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
          eyebrow="PURCHASE DETAIL"
          title={order.purchaseOrderNumber}
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
              <div><span>Supplier</span><strong>{order.supplier}</strong></div>
              <div><span>Warehouse</span><strong>{order.warehouse}</strong></div>
              <div><span>Status</span><strong>{order.status}</strong></div>
              <div><span>Total</span><strong>{order.currency} {order.grandTotal.toLocaleString()}</strong></div>
            </div>
            <table>
              <thead><tr><th>Product</th><th>Ordered</th><th>Received</th><th>Receive now</th><th>Unit cost</th><th>Line</th></tr></thead>
              <tbody>
                {order.lines.map(line => (
                  <tr key={line.id}>
                    <td>{line.product}</td>
                    <td>{line.quantity}</td>
                    <td>{line.receivedQuantity}</td>
                    <td>
                      {order.status === 'Approved' && line.outstandingQuantity > 0 ? (
                        <input
                          className="inline-qty"
                          type="number"
                          min="0.0001"
                          max={line.outstandingQuantity}
                          step="0.0001"
                          value={receiveQty[line.id] ?? String(line.outstandingQuantity)}
                          onChange={event => setReceiveQty(current => ({ ...current, [line.id]: event.target.value }))}
                        />
                      ) : '—'}
                    </td>
                    <td>{line.unitCost.toLocaleString()}</td>
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

function availableActions(status: string) {
  const actions: { key: string; label: string }[] = []
  if (status === 'Draft') actions.push({ key: 'approve', label: 'Approve' }, { key: 'cancel', label: 'Cancel' })
  if (status === 'Approved') actions.push({ key: 'receive', label: 'Receive goods' }, { key: 'cancel', label: 'Cancel' })
  return actions
}
