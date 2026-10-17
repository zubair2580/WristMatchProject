import { CheckCircle2, Plus, Search } from 'lucide-react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState, type FormEvent } from 'react'
import AdminModal from '../components/AdminModal'
import { adminMutation } from '../store/adminApi'
import { apiUrl } from '../store/api'

type InvoiceListItem = { id: string; invoice: string; order: string; customer: string; issued: string; due: string; total: number; balance: number; status: string; currency: string }
type InvoiceDetail = {
  id: string; invoiceNumber: string; salesOrderId: string; orderNumber?: string; customerId: string; customer: string; status: string
  currency: string; invoiceDate: string; dueDate: string; subtotal: number; taxTotal: number; grandTotal: number; outstandingAmount: number; notes?: string
  lines: { id: string; productId: string; product: string; quantity: number; unitPrice: number; taxAmount: number; lineTotal: number }[]
  payments: { id: string; paymentNumber: string; method: string; status: string; amount: number; currency: string; receivedAt: string }[]
}
type OrderOption = { id: string; order: string; customer: string; status: string; total: number; currency: string }

export default function AdminInvoicesPage() {
  const client = useQueryClient()
  const [search, setSearch] = useState('')
  const [creating, setCreating] = useState(false)
  const [detailId, setDetailId] = useState<string | null>(null)
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)
  const [payAmount, setPayAmount] = useState('')
  const [payMethod, setPayMethod] = useState('BankTransfer')
  const [form, setForm] = useState({ salesOrderId: '', notes: '', dueDate: '' })

  const list = useQuery<{ items: InvoiceListItem[]; total: number }>({
    queryKey: ['admin-invoices', search],
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/admin/invoices?search=${encodeURIComponent(search)}&page=1&pageSize=50`, { credentials: 'include' })
      if (!response.ok) throw new Error('Unable to load invoices')
      return response.json()
    },
  })
  const detail = useQuery<InvoiceDetail>({
    queryKey: ['admin-invoice', detailId],
    enabled: Boolean(detailId),
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/admin/invoices/${detailId}`, { credentials: 'include' })
      if (!response.ok) throw new Error('Unable to load invoice')
      return response.json()
    },
  })
  const orders = useQuery<{ items: OrderOption[] }>({
    queryKey: ['admin-orders-for-invoice'],
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/admin/orders?page=1&pageSize=100`, { credentials: 'include' })
      if (!response.ok) throw new Error('Unable to load orders')
      return response.json()
    },
  })

  const orderOptions = useMemo(
    () => (orders.data?.items ?? []).filter(item => item.status !== 'Cancelled' && item.status !== 'Draft'),
    [orders.data],
  )

  const openCreate = () => {
    setCreating(true)
    setError('')
    const first = orderOptions[0]
    setForm({
      salesOrderId: first?.id ?? '',
      notes: '',
      dueDate: new Date(Date.now() + 14 * 86400000).toISOString().slice(0, 10),
    })
  }

  const openDetail = (id: string) => {
    setDetailId(id)
    setError('')
    setPayAmount('')
    setPayMethod('BankTransfer')
  }

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setSaving(true)
    setError('')
    try {
      const created = await adminMutation<InvoiceDetail>('/admin/invoices', 'POST', JSON.stringify({
        salesOrderId: form.salesOrderId,
        notes: form.notes,
        dueDate: form.dueDate ? new Date(form.dueDate).toISOString() : null,
      }), { 'Content-Type': 'application/json' })
      await client.invalidateQueries({ queryKey: ['admin-invoices'] })
      setCreating(false)
      openDetail(created.id)
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Unable to create invoice.')
    } finally {
      setSaving(false)
    }
  }

  const transition = async (action: string) => {
    if (!detailId || !detail.data) return
    setSaving(true)
    setError('')
    try {
      const body: { action: string; amount?: number; method?: string } = { action }
      if (action === 'pay') {
        const amount = Number(payAmount || detail.data.outstandingAmount)
        body.amount = amount
        body.method = payMethod
      }
      await adminMutation(`/admin/invoices/${detailId}/transitions`, 'POST', JSON.stringify(body), { 'Content-Type': 'application/json' })
      await client.invalidateQueries({ queryKey: ['admin-invoice', detailId] })
      await client.invalidateQueries({ queryKey: ['admin-invoices'] })
      await client.invalidateQueries({ queryKey: ['admin-orders'] })
      setPayAmount('')
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Unable to update invoice.')
    } finally {
      setSaving(false)
    }
  }

  const invoice = detail.data
  const actions = invoice ? availableActions(invoice.status, invoice.payments.length) : []

  return (
    <div>
      <div className="page-heading">
        <div>
          <p className="eyebrow">ACCOUNTS RECEIVABLE</p>
          <h1>Invoices</h1>
          <p>Create invoices from sales orders, issue them, and allocate partial or full payments against outstanding balance.</p>
        </div>
        <button className="primary-button compact-button" onClick={openCreate}><Plus size={17} /> New invoice</button>
      </div>
      <section className="panel table-panel">
        <div className="table-toolbar">
          <div className="search-box"><Search size={17} /><input value={search} onChange={event => setSearch(event.target.value)} placeholder="Search invoice, order, customer or status" /></div>
        </div>
        <div className="table-scroll">
          <table>
            <thead>
              <tr><th>Sr #</th><th>Invoice</th><th>Order</th><th>Customer</th><th>Issued</th><th>Due</th><th>Total</th><th>Balance</th><th>Status</th></tr>
            </thead>
            <tbody>
              {(list.data?.items ?? []).map((item, index) => (
                <tr key={item.id} className="clickable-row" onClick={() => openDetail(item.id)}>
                  <td>{index + 1}</td>
                  <td><strong>{item.invoice}</strong></td>
                  <td>{item.order}</td>
                  <td>{item.customer}</td>
                  <td>{new Date(item.issued).toLocaleDateString()}</td>
                  <td>{new Date(item.due).toLocaleDateString()}</td>
                  <td>{item.currency} {Number(item.total).toLocaleString()}</td>
                  <td>{item.currency} {Number(item.balance).toLocaleString()}</td>
                  <td><span className="status-pill"><i /> {item.status}</span></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {list.isPending && <div className="module-empty"><h2>Loading invoices...</h2></div>}
        {!list.isPending && (list.data?.items.length ?? 0) === 0 && <div className="module-empty"><div className="module-empty-mark">W</div><h2>No invoices yet</h2><p>Create an invoice from a confirmed sales order.</p></div>}
        <div className="table-pagination"><span>{list.data?.total ?? 0} invoices</span></div>
      </section>

      {creating && (
        <AdminModal
          eyebrow="SALES INVOICE"
          title="Create invoice"
          onClose={() => setCreating(false)}
          footer={<><button className="outline-button" onClick={() => setCreating(false)}>Cancel</button><button className="primary-button compact-button" form="invoice-form" disabled={saving || !form.salesOrderId}>{saving ? 'Creating...' : 'Create draft'}</button></>}
        >
          <form id="invoice-form" className="admin-form" onSubmit={submit}>
            <div className="form-grid">
              <label className="form-span">Sales order
                <select required value={form.salesOrderId} onChange={event => setForm(current => ({ ...current, salesOrderId: event.target.value }))}>
                  {orderOptions.length === 0 && <option value="">No billable orders</option>}
                  {orderOptions.map(item => <option key={item.id} value={item.id}>{item.order} · {item.customer} · {item.currency} {item.total.toLocaleString()} · {item.status}</option>)}
                </select>
              </label>
              <label>Due date<input type="date" value={form.dueDate} onChange={event => setForm(current => ({ ...current, dueDate: event.target.value }))} /></label>
              <label className="form-span">Notes<textarea rows={2} value={form.notes} onChange={event => setForm(current => ({ ...current, notes: event.target.value }))} /></label>
            </div>
            {error && <div className="form-error">{error}</div>}
          </form>
        </AdminModal>
      )}

      {detailId && invoice && (
        <AdminModal
          eyebrow="INVOICE DETAIL"
          title={invoice.invoiceNumber}
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
              <div><span>Customer</span><strong>{invoice.customer}</strong></div>
              <div><span>Order</span><strong>{invoice.orderNumber ?? '—'}</strong></div>
              <div><span>Status</span><strong>{invoice.status}</strong></div>
              <div><span>Outstanding</span><strong>{invoice.currency} {invoice.outstandingAmount.toLocaleString()}</strong></div>
              <div><span>Due</span><strong>{new Date(invoice.dueDate).toLocaleDateString()}</strong></div>
              <div><span>Total</span><strong>{invoice.currency} {invoice.grandTotal.toLocaleString()}</strong></div>
            </div>
            <table>
              <thead><tr><th>Product</th><th>Qty</th><th>Unit</th><th>Tax</th><th>Line</th></tr></thead>
              <tbody>
                {invoice.lines.map(line => (
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
              <span>Subtotal {invoice.subtotal.toLocaleString()}</span>
              <span>Tax {invoice.taxTotal.toLocaleString()}</span>
              <strong>Grand {invoice.grandTotal.toLocaleString()}</strong>
            </div>
            {invoice.status === 'Issued' && invoice.outstandingAmount > 0 && (
              <div className="form-grid" style={{ marginTop: 16 }}>
                <label>Payment amount
                  <input className="inline-qty" style={{ width: '100%' }} type="number" min="0.01" max={invoice.outstandingAmount} step="0.01" value={payAmount || String(invoice.outstandingAmount)} onChange={event => setPayAmount(event.target.value)} />
                </label>
                <label>Method
                  <select value={payMethod} onChange={event => setPayMethod(event.target.value)}>
                    <option value="BankTransfer">Bank transfer</option>
                    <option value="CashOnDelivery">Cash on delivery</option>
                    <option value="Card">Card</option>
                  </select>
                </label>
              </div>
            )}
            {invoice.payments.length > 0 && (
              <table style={{ marginTop: 16 }}>
                <thead><tr><th>Payment</th><th>Method</th><th>Amount</th><th>Received</th><th>Status</th></tr></thead>
                <tbody>
                  {invoice.payments.map(payment => (
                    <tr key={payment.id}>
                      <td>{payment.paymentNumber}</td>
                      <td>{payment.method}</td>
                      <td>{payment.currency} {payment.amount.toLocaleString()}</td>
                      <td>{new Date(payment.receivedAt).toLocaleDateString()}</td>
                      <td>{payment.status}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
            {error && <div className="form-error">{error}</div>}
          </div>
        </AdminModal>
      )}
    </div>
  )
}

function availableActions(status: string, paymentCount: number) {
  const actions: { key: string; label: string }[] = []
  if (status === 'Draft') actions.push({ key: 'issue', label: 'Issue invoice' }, { key: 'cancel', label: 'Cancel' })
  if (status === 'Issued') {
    actions.push({ key: 'pay', label: 'Record payment' })
    if (paymentCount === 0) actions.push({ key: 'cancel', label: 'Cancel' })
  }
  return actions
}
