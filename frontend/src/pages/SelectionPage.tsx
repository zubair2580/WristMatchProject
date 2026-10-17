import { ArrowRight, Check, Heart, Lock, Minus, Plus, ShieldCheck, Trash2 } from 'lucide-react'
import { useMemo, useState, type FormEvent, type ReactNode } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { getProducts } from '../store/api'
import { placeCheckoutOrder } from '../store/checkoutApi'
import { colourHex } from '../store/productExtras'
import { useStore, type PaymentMethod } from '../store/StoreContext'

function VisaMark() {
  return (
    <span className="pay-mark visa" aria-label="Visa">
      <svg viewBox="0 0 48 16" width="40" height="14"><path fill="#1A1F71" d="M17.4 1.2h-3.5l-3.5 13.6h3.5l3.5-13.6zm14.2 8.7c0-3.4-4.7-3.6-4.7-5.1.0-1 1-1.3 1.9-1.3 1.1 0 2 .3 2.9.8l.5-2.8A9.4 9.4 0 0 0 28.6.8c-3.4 0-5.8 1.8-5.8 4.4 0 1.9 1.7 3 3 3.6 1.3.7 1.8 1.1 1.8 1.7 0 .9-1.1 1.3-2.1 1.3-1.4 0-2.7-.4-3.8-1l-.6 2.8c1.2.5 2.7.8 4.3.8 3.6.1 6.2-1.8 6.2-4.7zm9.1 4.9h3l-2.6-13.6h-2.8c-.7 0-1.3.4-1.6 1.1L31 14.8h3.5l.9-2.5h4.3l.5 2.5zm-3.7-4.9.4-1.2c.1-.3.5-1.5.5-1.5s.2.4.3.8l.5 1.9h-1.7zm-17.7-8.7L16.5 14.8h-3.6L10.5 4.3c-.2-.7-.3-1-.9-1.3A12 12 0 0 0 6.2 1.7l.1-.5h5.7c.7 0 1.4.5 1.6 1.3l1.4 7.6 3.6-8.9h3.6z"/></svg>
    </span>
  )
}

function MastercardMark() {
  return (
    <span className="pay-mark mastercard" aria-label="Mastercard">
      <svg viewBox="0 0 36 22" width="34" height="20">
        <circle cx="13" cy="11" r="9" fill="#EB001B" />
        <circle cx="23" cy="11" r="9" fill="#F79E1B" />
        <path d="M18 4.2a9 9 0 0 1 0 13.6 9 9 0 0 1 0-13.6z" fill="#FF5F00" />
      </svg>
    </span>
  )
}

function JazzCashMark() {
  return <span className="pay-mark jazzcash" aria-label="JazzCash">JazzCash</span>
}

function EasyPaisaMark() {
  return <span className="pay-mark easypaisa" aria-label="EasyPaisa">easy<span>paisa</span></span>
}

function BankMark() {
  return (
    <span className="pay-mark bank" aria-label="Bank">
      <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" strokeWidth="1.7">
        <path d="M3 10h18M5 10v8M9 10v8M15 10v8M19 10v8M2 18h20M12 3l9 5H3l9-5z" />
      </svg>
    </span>
  )
}

function CodMark() {
  return (
    <span className="pay-mark cod" aria-label="Cash on delivery">
      <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" strokeWidth="1.7">
        <rect x="2" y="6" width="20" height="12" rx="2" />
        <circle cx="12" cy="12" r="2.5" />
        <path d="M6 12h.01M18 12h.01" />
      </svg>
    </span>
  )
}

const gateways: { id: PaymentMethod; title: string; detail: string; badges: ReactNode }[] = [
  { id: 'card', title: 'Card / wallet', detail: 'Visa, Mastercard, JazzCash & EasyPaisa', badges: (<><VisaMark /><MastercardMark /><JazzCashMark /><EasyPaisaMark /></>) },
  { id: 'bank', title: 'Bank transfer', detail: 'IBFT / deposit · held 24 hours', badges: (<BankMark />) },
  { id: 'cod', title: 'Cash on delivery', detail: 'Pay the courier on arrival', badges: (<CodMark />) },
]

const emptyForm = {
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  addressLine1: '',
  addressLine2: '',
  city: 'Karachi',
  stateProvince: 'Sindh',
  postalCode: '',
  notes: '',
  createAccount: true,
  password: '',
}

export default function SelectionPage() {
  const navigate = useNavigate()
  const { cart, cartCount, setCartQuantity, removeFromCart, clearCart, addToCart, paymentMethod, setPaymentMethod } = useStore()
  const { data } = useQuery({ queryKey: ['selection-products'], queryFn: () => getProducts('?page=1&pageSize=50') })
  const [form, setForm] = useState(emptyForm)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

  const lines = useMemo(() => {
    const products = data?.items ?? []
    return cart.map(line => {
      const product = products.find(item => item.id === line.id)
      return product ? { ...line, product } : null
    }).filter(Boolean) as { id: string; quantity: number; product: NonNullable<typeof data>['items'][number] }[]
  }, [cart, data])

  const suggestions = useMemo(() => {
    const inCart = new Set(cart.map(line => line.id))
    return (data?.items ?? []).filter(item => !inCart.has(item.id)).slice(0, 4)
  }, [cart, data])

  const subtotal = lines.reduce((sum, line) => sum + line.product.price * line.quantity, 0)
  const shipping = lines.length === 0 ? 0 : subtotal >= 50000 ? 0 : 500
  const tax = Math.round(subtotal * 0.1)
  const total = subtotal + shipping + tax
  const currency = lines[0]?.product.currency ?? 'PKR'
  const selectedGateway = gateways.find(item => item.id === paymentMethod) ?? gateways[0]

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    if (lines.length === 0) return
    setSubmitting(true)
    setError('')
    try {
      const order = await placeCheckoutOrder({
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        email: form.email.trim(),
        phone: form.phone.trim(),
        paymentMethod,
        notes: form.notes.trim() || undefined,
        createAccount: form.createAccount,
        password: form.createAccount ? form.password : undefined,
        shippingAddress: {
          recipientName: `${form.firstName.trim()} ${form.lastName.trim()}`,
          phone: form.phone.trim(),
          addressLine1: form.addressLine1.trim(),
          addressLine2: form.addressLine2.trim() || undefined,
          city: form.city.trim(),
          stateProvince: form.stateProvince.trim() || undefined,
          postalCode: form.postalCode.trim() || undefined,
          countryCode: 'PK',
        },
        lines: lines.map(line => ({ productId: line.id, quantity: line.quantity })),
      })
      clearCart()
      if (paymentMethod === 'card') navigate(`/pay/${order.id}`)
      else navigate(`/order/${order.id}`)
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Unable to place order.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <main className="selection-page cart-page checkout-rich">
      <header className="checkout-top">
        <div>
          <p className="eyebrow">CHECKOUT</p>
          <h1>Your bag</h1>
        </div>
        <div className="checkout-trust">
          <span><Lock size={14} /> Secure checkout</span>
          <span><ShieldCheck size={14} /> Orders saved to admin</span>
        </div>
      </header>

      {lines.length === 0 ? (
        <div className="empty-results selection-empty">
          <Heart size={25} />
          <h2>Your bag is empty</h2>
          <Link className="primary-button" to="/catalog">Browse watches <ArrowRight size={17} /></Link>
          <Link className="text-link" to="/account">My orders <ArrowRight size={14} /></Link>
        </div>
      ) : (
        <>
          <form className="checkout-layout" onSubmit={submit}>
            <section className="checkout-main">
              <div className="checkout-lines">
                <div className="cart-lines-head">
                  <h2>{cartCount} {cartCount === 1 ? 'item' : 'items'}</h2>
                  <button type="button" className="filter-reset" onClick={clearCart}>Clear bag</button>
                </div>
                {lines.map(line => (
                  <article className="checkout-line" key={line.id}>
                    <Link className="checkout-line-media" to={`/products/${line.id}`}>
                      <img src={line.product.imageUrl} alt={line.product.name} />
                    </Link>
                    <div className="checkout-line-body">
                      <div className="checkout-line-copy">
                        <p><i className="colour-dot inline" style={{ background: colourHex(line.product.colour) }} />{line.product.colour} · {line.product.material}</p>
                        <h3><Link to={`/products/${line.id}`}>{line.product.name}</Link></h3>
                        <strong>{line.product.currency} {line.product.price.toLocaleString()} each</strong>
                      </div>
                      <div className="checkout-line-tools">
                        <div className="qty-control">
                          <button type="button" aria-label="Decrease quantity" onClick={() => setCartQuantity(line.id, line.quantity - 1)}><Minus size={15} /></button>
                          <span>{line.quantity}</span>
                          <button type="button" aria-label="Increase quantity" onClick={() => setCartQuantity(line.id, line.quantity + 1)}><Plus size={15} /></button>
                        </div>
                        <strong>{line.product.currency} {(line.product.price * line.quantity).toLocaleString()}</strong>
                        <button type="button" className="remove-button" onClick={() => removeFromCart(line.id)} aria-label={`Remove ${line.product.name}`}><Trash2 size={17} /></button>
                      </div>
                    </div>
                  </article>
                ))}
              </div>

              <div className="checkout-form-card">
                <p className="eyebrow">CUSTOMER DETAILS</p>
                <h2>Contact & delivery</h2>
                <div className="checkout-form-grid">
                  <label>First name<input required value={form.firstName} onChange={event => setForm(current => ({ ...current, firstName: event.target.value }))} /></label>
                  <label>Last name<input required value={form.lastName} onChange={event => setForm(current => ({ ...current, lastName: event.target.value }))} /></label>
                  <label>Email<input required type="email" value={form.email} onChange={event => setForm(current => ({ ...current, email: event.target.value }))} /></label>
                  <label>Phone<input required value={form.phone} onChange={event => setForm(current => ({ ...current, phone: event.target.value }))} placeholder="+92 300 ..." /></label>
                  <label className="span-2">Address line 1<input required value={form.addressLine1} onChange={event => setForm(current => ({ ...current, addressLine1: event.target.value }))} /></label>
                  <label className="span-2">Address line 2<input value={form.addressLine2} onChange={event => setForm(current => ({ ...current, addressLine2: event.target.value }))} /></label>
                  <label>City<input required value={form.city} onChange={event => setForm(current => ({ ...current, city: event.target.value }))} /></label>
                  <label>Province<input value={form.stateProvince} onChange={event => setForm(current => ({ ...current, stateProvince: event.target.value }))} /></label>
                  <label>Postal code<input value={form.postalCode} onChange={event => setForm(current => ({ ...current, postalCode: event.target.value }))} /></label>
                  <label className="span-2">Order note<textarea rows={3} value={form.notes} onChange={event => setForm(current => ({ ...current, notes: event.target.value }))} /></label>
                </div>
                <label className="checkout-account-toggle">
                  <input type="checkbox" checked={form.createAccount} onChange={event => setForm(current => ({ ...current, createAccount: event.target.checked }))} />
                  <span>Create customer account to track this order</span>
                </label>
                {form.createAccount && (
                  <label>Account password<input required={form.createAccount} type="password" minLength={8} value={form.password} onChange={event => setForm(current => ({ ...current, password: event.target.value }))} placeholder="At least 8 characters" /></label>
                )}
              </div>
            </section>

            <aside className="checkout-aside">
              <div className="checkout-summary-card">
                <p className="eyebrow">ORDER SUMMARY</p>
                <div className="summary-rows">
                  <div><span>Subtotal</span><strong>{currency} {subtotal.toLocaleString()}</strong></div>
                  <div><span>Shipping</span><strong>{shipping === 0 ? 'Free' : `${currency} ${shipping.toLocaleString()}`}</strong></div>
                  <div><span>Estimated tax</span><strong>{currency} {tax.toLocaleString()}</strong></div>
                  <div className="summary-total"><span>Total</span><strong>{currency} {total.toLocaleString()}</strong></div>
                </div>
              </div>

              <div className="checkout-pay-card">
                <div className="checkout-pay-head">
                  <p className="eyebrow">PAYMENT</p>
                  <span>Select a gateway</span>
                </div>
                <div className="pay-gateway-list" role="radiogroup" aria-label="Payment method">
                  {gateways.map(gateway => (
                    <button
                      key={gateway.id}
                      type="button"
                      role="radio"
                      aria-checked={paymentMethod === gateway.id}
                      className={paymentMethod === gateway.id ? 'pay-gateway active' : 'pay-gateway'}
                      onClick={() => setPaymentMethod(gateway.id)}
                    >
                      <span className="pay-radio">{paymentMethod === gateway.id && <Check size={12} />}</span>
                      <span className="pay-gateway-copy">
                        <strong>{gateway.title}</strong>
                        <small>{gateway.detail}</small>
                      </span>
                      <span className="pay-badges">{gateway.badges}</span>
                    </button>
                  ))}
                </div>

                <div className="pay-selected-note">
                  <Lock size={14} />
                  <span>
                    {paymentMethod === 'card' && 'You will be redirected to the card gateway.'}
                    {paymentMethod === 'bank' && 'Bank transfer details appear after confirmation.'}
                    {paymentMethod === 'cod' && 'Cash collected on delivery after confirmation.'}
                    {' '}Paying with <strong>{selectedGateway.title}</strong>
                  </span>
                </div>

                {error && <p className="checkout-error">{error}</p>}

                <button type="submit" className="primary-button checkout-place-btn" disabled={submitting}>
                  {submitting ? 'Placing order...' : <><span>Place order</span><em>{currency} {total.toLocaleString()}</em><ArrowRight size={17} /></>}
                </button>
                <Link className="text-link checkout-continue" to="/account">Already ordered? View account <ArrowRight size={15} /></Link>
              </div>
            </aside>
          </form>

          {suggestions.length > 0 && (
            <section className="checkout-suggestions">
              <div className="home-section-head">
                <div>
                  <p className="eyebrow">ADD MORE</p>
                  <h2>You may also like</h2>
                </div>
              </div>
              <div className="checkout-suggest-grid">
                {suggestions.map(product => (
                  <article className="checkout-suggest-card" key={product.id}>
                    <Link className="checkout-suggest-media" to={`/products/${product.id}`}>
                      <img src={product.imageUrl} alt={product.name} />
                    </Link>
                    <div className="checkout-suggest-body">
                      <Link to={`/products/${product.id}`}><h3>{product.name}</h3></Link>
                      <div className="card-bottom">
                        <strong>{product.currency} {product.price.toLocaleString()}</strong>
                        <button type="button" className="selection-button" onClick={() => addToCart(product.id)}>Add</button>
                      </div>
                    </div>
                  </article>
                ))}
              </div>
            </section>
          )}
        </>
      )}
    </main>
  )
}
