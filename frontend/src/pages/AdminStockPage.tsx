import { Plus, Search } from 'lucide-react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useState, type FormEvent } from 'react'
import AdminModal from '../components/AdminModal'
import { adminMutation } from '../store/adminApi'
import { apiUrl } from '../store/api'

type BalanceRow = Record<string, unknown>
type Option = { id: string; label: string }

export default function AdminStockPage() {
  const client = useQueryClient()
  const [search, setSearch] = useState('')
  const [open, setOpen] = useState(false)
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)
  const [form, setForm] = useState({ warehouseId: '', productId: '', quantityDelta: '1', unitCost: '', reference: '', notes: '' })

  const balances = useQuery<{ items: BalanceRow[]; total: number }>({
    queryKey: ['admin-stock', search],
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/admin/stock?search=${encodeURIComponent(search)}&page=1&pageSize=100`, { credentials: 'include' })
      if (!response.ok) throw new Error('Unable to load stock balances')
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
  const products = useQuery<{ id: string; name: string; sku?: string; isActive: boolean }[]>({
    queryKey: ['admin-products'],
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/admin/products`, { credentials: 'include' })
      if (!response.ok) throw new Error('Unable to load products')
      return response.json()
    },
  })

  const warehouseOptions: Option[] = (warehouses.data?.items ?? []).filter(item => item.isActive).map(item => ({ id: item.id, label: `${item.code} · ${item.name}` }))
  const productOptions: Option[] = (products.data ?? []).filter(item => item.isActive).map(item => ({ id: item.id, label: item.name }))
  const columns = ['SKU', 'Product', 'Warehouse', 'On hand', 'Reserved', 'Available', 'Average cost']

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setSaving(true)
    setError('')
    try {
      await adminMutation('/admin/stock/adjustments', 'POST', JSON.stringify({
        warehouseId: form.warehouseId,
        productId: form.productId,
        quantityDelta: Number(form.quantityDelta),
        unitCost: form.unitCost ? Number(form.unitCost) : null,
        reference: form.reference,
        notes: form.notes,
      }), { 'Content-Type': 'application/json' })
      await client.invalidateQueries({ queryKey: ['admin-stock'] })
      setOpen(false)
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Unable to post adjustment.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div>
      <div className="page-heading">
        <div>
          <p className="eyebrow">INVENTORY CONTROL</p>
          <h1>Stock</h1>
          <p>Live warehouse balances with weighted-average cost. Post adjustments to update on-hand quantities.</p>
        </div>
        <button className="primary-button compact-button" onClick={() => { setOpen(true); setError(''); setForm({ warehouseId: warehouseOptions[0]?.id ?? '', productId: productOptions[0]?.id ?? '', quantityDelta: '1', unitCost: '', reference: '', notes: '' }) }}>
          <Plus size={17} /> Stock adjustment
        </button>
      </div>
      <section className="panel table-panel">
        <div className="table-toolbar">
          <div className="search-box"><Search size={17} /><input value={search} onChange={event => setSearch(event.target.value)} placeholder="Search SKU, product or warehouse" /></div>
        </div>
        <div className="table-scroll">
          <table>
            <thead><tr><th>Sr #</th>{columns.map(column => <th key={column}>{column}</th>)}</tr></thead>
            <tbody>
              {(balances.data?.items ?? []).map((row, index) => (
                <tr key={index}>
                  <td>{index + 1}</td>
                  {columns.map(column => <td key={column}>{formatValue(row[column])}</td>)}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {balances.isPending && <div className="module-empty"><h2>Loading balances...</h2></div>}
        {!balances.isPending && (balances.data?.items.length ?? 0) === 0 && (
          <div className="module-empty">
            <div className="module-empty-mark">W</div>
            <h2>No stock balances yet</h2>
            <p>Post a positive stock adjustment to create the first warehouse balance.</p>
          </div>
        )}
        <div className="table-pagination"><span>{balances.data?.total ?? 0} balances</span></div>
      </section>
      {open && (
        <AdminModal
          eyebrow="INVENTORY"
          title="Stock adjustment"
          onClose={() => setOpen(false)}
          footer={<><button className="outline-button" onClick={() => setOpen(false)}>Cancel</button><button className="primary-button compact-button" form="stock-form" disabled={saving}>{saving ? 'Posting...' : 'Post adjustment'}</button></>}
        >
          <form id="stock-form" className="admin-form" onSubmit={submit}>
            <div className="form-grid">
              <label>Warehouse
                <select required value={form.warehouseId} onChange={event => setForm(current => ({ ...current, warehouseId: event.target.value }))}>
                  {warehouseOptions.map(item => <option key={item.id} value={item.id}>{item.label}</option>)}
                </select>
              </label>
              <label>Product
                <select required value={form.productId} onChange={event => setForm(current => ({ ...current, productId: event.target.value }))}>
                  {productOptions.map(item => <option key={item.id} value={item.id}>{item.label}</option>)}
                </select>
              </label>
              <label>Quantity delta<input required type="number" step="0.0001" value={form.quantityDelta} onChange={event => setForm(current => ({ ...current, quantityDelta: event.target.value }))} /></label>
              <label>Unit cost (required for increases)<input type="number" min="0" step="0.01" value={form.unitCost} onChange={event => setForm(current => ({ ...current, unitCost: event.target.value }))} /></label>
              <label>Reference<input value={form.reference} onChange={event => setForm(current => ({ ...current, reference: event.target.value }))} /></label>
              <label className="form-span">Notes<textarea rows={3} value={form.notes} onChange={event => setForm(current => ({ ...current, notes: event.target.value }))} /></label>
            </div>
            {error && <div className="form-error">{error}</div>}
          </form>
        </AdminModal>
      )}
    </div>
  )
}

function formatValue(value: unknown) {
  if (value === null || value === undefined) return 'Not provided'
  if (typeof value === 'number') return value.toLocaleString()
  return String(value)
}
