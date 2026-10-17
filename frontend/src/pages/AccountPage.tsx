import { ArrowRight, LogOut, Package, UserRound } from 'lucide-react'
import { useEffect, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { getCustomerMe, getCustomerOrders, loginCustomer, logoutCustomer, registerCustomer, type CustomerAccount, type CustomerOrderSummary } from '../store/checkoutApi'

export default function AccountPage() {
  const [mode, setMode] = useState<'login' | 'register'>('login')
  const [account, setAccount] = useState<CustomerAccount | null>(null)
  const [orders, setOrders] = useState<CustomerOrderSummary[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [form, setForm] = useState({ firstName: '', lastName: '', email: '', phone: '', password: '' })

  const refresh = async () => {
    setLoading(true)
    setError('')
    try {
      const me = await getCustomerMe()
      setAccount(me)
      setOrders(me ? await getCustomerOrders() : [])
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Unable to load account.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { void refresh() }, [])

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setError('')
    try {
      const next = mode === 'login'
        ? await loginCustomer(form.email, form.password)
        : await registerCustomer(form)
      setAccount(next)
      setOrders(await getCustomerOrders())
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Unable to sign in.')
    }
  }

  const signOut = async () => {
    await logoutCustomer()
    setAccount(null)
    setOrders([])
  }

  if (loading) return <main className="account-page"><div className="status-card">Loading account...</div></main>

  if (!account) {
    return (
      <main className="account-page">
        <section className="account-auth-card">
          <p className="eyebrow">CUSTOMER ACCOUNT</p>
          <h1>{mode === 'login' ? 'Sign in' : 'Create account'}</h1>
          <div className="account-tabs">
            <button type="button" className={mode === 'login' ? 'active' : ''} onClick={() => setMode('login')}>Sign in</button>
            <button type="button" className={mode === 'register' ? 'active' : ''} onClick={() => setMode('register')}>Register</button>
          </div>
          <form onSubmit={submit}>
            {mode === 'register' && (
              <>
                <label>First name<input required value={form.firstName} onChange={event => setForm(current => ({ ...current, firstName: event.target.value }))} /></label>
                <label>Last name<input required value={form.lastName} onChange={event => setForm(current => ({ ...current, lastName: event.target.value }))} /></label>
                <label>Phone<input required value={form.phone} onChange={event => setForm(current => ({ ...current, phone: event.target.value }))} /></label>
              </>
            )}
            <label>Email<input required type="email" value={form.email} onChange={event => setForm(current => ({ ...current, email: event.target.value }))} /></label>
            <label>Password<input required type="password" minLength={8} value={form.password} onChange={event => setForm(current => ({ ...current, password: event.target.value }))} /></label>
            {error && <p className="checkout-error">{error}</p>}
            <button className="primary-button" type="submit">{mode === 'login' ? 'Sign in' : 'Create account'}</button>
          </form>
        </section>
      </main>
    )
  }

  return (
    <main className="account-page">
      <header className="account-hero">
        <div>
          <p className="eyebrow">MY ACCOUNT</p>
          <h1>{account.firstName} {account.lastName}</h1>
          <p>{account.email} · {account.customerNumber}</p>
        </div>
        <button type="button" className="outline-button" onClick={signOut}><LogOut size={15} /> Sign out</button>
      </header>

      <section className="account-panel">
        <div className="account-panel-head">
          <h2><Package size={18} /> Orders</h2>
          <Link className="text-link" to="/catalog">Shop more <ArrowRight size={14} /></Link>
        </div>
        {orders.length === 0 ? (
          <div className="account-empty"><UserRound size={22} /><p>No orders yet.</p></div>
        ) : (
          <div className="account-orders">
            {orders.map(order => (
              <article key={order.id}>
                <div>
                  <strong>{order.orderNumber}</strong>
                  <span>{new Date(order.placedAt).toLocaleString()}</span>
                </div>
                <div>
                  <span>{order.status}</span>
                  <span>{order.paymentStatus}</span>
                </div>
                <strong>{order.currency} {order.grandTotal.toLocaleString()}</strong>
                <Link className="text-link" to={`/order/${order.id}`}>Details <ArrowRight size={14} /></Link>
              </article>
            ))}
          </div>
        )}
      </section>
    </main>
  )
}
