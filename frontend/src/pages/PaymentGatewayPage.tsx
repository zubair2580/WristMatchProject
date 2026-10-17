import { ArrowRight, CreditCard, Lock, ShieldCheck } from 'lucide-react'
import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { completeCheckoutPayment, getCheckoutOrder } from '../store/checkoutApi'

function VisaIcon() {
  return (
    <span className="pay-brand visa" title="Visa" aria-label="Visa">
      <svg viewBox="0 0 48 16" width="42" height="14" aria-hidden="true">
        <path fill="#1A1F71" d="M17.4 1.2h-3.5l-3.5 13.6h3.5l3.5-13.6zm14.2 8.7c0-3.4-4.7-3.6-4.7-5.1 0-1 1-1.3 1.9-1.3 1.1 0 2 .3 2.9.8l.5-2.8A9.4 9.4 0 0 0 28.6.8c-3.4 0-5.8 1.8-5.8 4.4 0 1.9 1.7 3 3 3.6 1.3.7 1.8 1.1 1.8 1.7 0 .9-1.1 1.3-2.1 1.3-1.4 0-2.7-.4-3.8-1l-.6 2.8c1.2.5 2.7.8 4.3.8 3.6.1 6.2-1.8 6.2-4.7zm9.1 4.9h3l-2.6-13.6h-2.8c-.7 0-1.3.4-1.6 1.1L31 14.8h3.5l.9-2.5h4.3l.5 2.5zm-3.7-4.9.4-1.2c.1-.3.5-1.5.5-1.5s.2.4.3.8l.5 1.9h-1.7zm-17.7-8.7L16.5 14.8h-3.6L10.5 4.3c-.2-.7-.3-1-.9-1.3A12 12 0 0 0 6.2 1.7l.1-.5h5.7c.7 0 1.4.5 1.6 1.3l1.4 7.6 3.6-8.9h3.6z" />
      </svg>
    </span>
  )
}

function MastercardIcon() {
  return (
    <span className="pay-brand mastercard" title="Mastercard" aria-label="Mastercard">
      <svg viewBox="0 0 36 22" width="36" height="22" aria-hidden="true">
        <circle cx="13" cy="11" r="9" fill="#EB001B" />
        <circle cx="23" cy="11" r="9" fill="#F79E1B" />
        <path d="M18 4.2a9 9 0 0 1 0 13.6 9 9 0 0 1 0-13.6z" fill="#FF5F00" />
      </svg>
    </span>
  )
}

function JazzCashIcon() {
  return (
    <span className="pay-brand jazzcash" title="JazzCash" aria-label="JazzCash">
      Jazz<span>Cash</span>
    </span>
  )
}

function EasyPaisaIcon() {
  return (
    <span className="pay-brand easypaisa" title="EasyPaisa" aria-label="EasyPaisa">
      easy<span>paisa</span>
    </span>
  )
}

function formatCardNumber(value: string) {
  return value.replace(/\D/g, '').slice(0, 16).replace(/(\d{4})(?=\d)/g, '$1 ').trim()
}

function formatExpiry(value: string) {
  const digits = value.replace(/\D/g, '').slice(0, 4)
  if (digits.length <= 2) return digits
  return `${digits.slice(0, 2)}/${digits.slice(2)}`
}

export default function PaymentGatewayPage() {
  const { id = '' } = useParams()
  const navigate = useNavigate()
  const [cardName, setCardName] = useState('')
  const [cardNumber, setCardNumber] = useState('')
  const [expiry, setExpiry] = useState('')
  const [cvc, setCvc] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  const { data: order, isError, isPending } = useQuery({
    queryKey: ['checkout-order', id],
    queryFn: () => getCheckoutOrder(id),
    enabled: Boolean(id),
  })

  const pay = async (success: boolean) => {
    if (!id) return
    if (success && (!cardName.trim() || cardNumber.replace(/\s/g, '').length < 12 || expiry.length < 4 || cvc.length < 3)) {
      setError('Enter cardholder name, card number, expiry, and CVC to continue.')
      return
    }
    setBusy(true)
    setError('')
    try {
      await completeCheckoutPayment(id, success)
      navigate(`/order/${id}`)
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Payment failed.')
    } finally {
      setBusy(false)
    }
  }

  if (isPending) return <main className="pay-gateway-page"><div className="status-card">Opening secure gateway...</div></main>
  if (isError || !order) {
    return (
      <main className="pay-gateway-page">
        <div className="status-card error-card">Unable to load this payment session. <Link to="/account">Sign in to your account</Link></div>
      </main>
    )
  }

  return (
    <main className="pay-gateway-page">
      <section className="pay-gateway-shell">
        <header className="pay-gateway-brand">
          <div>
            <p className="eyebrow">WRISTMATCH PAY</p>
            <h1>Secure card checkout</h1>
            <p className="pay-gateway-sub">Complete payment for order {order.orderNumber}</p>
          </div>
          <div className="pay-gateway-secure"><Lock size={15} /> Encrypted demo gateway</div>
        </header>

        <div className="pay-gateway-grid">
          <form className="pay-gateway-form" onSubmit={event => { event.preventDefault(); void pay(true) }}>
            <div className="pay-gateway-methods">
              <p className="pay-methods-label">Accepted methods</p>
              <div className="pay-gateway-marks" role="list">
                <VisaIcon />
                <MastercardIcon />
                <JazzCashIcon />
                <EasyPaisaIcon />
              </div>
            </div>

            <div className="pay-card-panel">
              <label>Name on card
                <input required autoComplete="cc-name" value={cardName} onChange={event => setCardName(event.target.value)} placeholder="As printed on card" />
              </label>
              <label>Card number
                <input required inputMode="numeric" autoComplete="cc-number" value={cardNumber} onChange={event => setCardNumber(formatCardNumber(event.target.value))} placeholder="4242 4242 4242 4242" />
              </label>
              <div className="pay-gateway-row">
                <label>Expiry
                  <input required inputMode="numeric" autoComplete="cc-exp" value={expiry} onChange={event => setExpiry(formatExpiry(event.target.value))} placeholder="MM/YY" />
                </label>
                <label>CVC
                  <input required inputMode="numeric" autoComplete="cc-csc" value={cvc} onChange={event => setCvc(event.target.value.replace(/\D/g, '').slice(0, 4))} placeholder="123" />
                </label>
              </div>
            </div>

            {error && <p className="checkout-error">{error}</p>}

            <button className="primary-button pay-submit" type="submit" disabled={busy}>
              <CreditCard size={17} />
              <span>{busy ? 'Processing...' : 'Pay now'}</span>
              {!busy && <em>{order.currency} {order.grandTotal.toLocaleString()}</em>}
            </button>
            <button className="outline-button pay-cancel" type="button" disabled={busy} onClick={() => void pay(false)}>Cancel payment</button>
            <p className="pay-gateway-note"><ShieldCheck size={14} /> Demo only — card numbers are not stored or charged.</p>
          </form>

          <aside className="pay-gateway-summary">
            <p className="eyebrow">ORDER</p>
            <strong>{order.orderNumber}</strong>
            <div className="pay-summary-rows">
              <div><span>Customer</span><span>{order.customer.firstName} {order.customer.lastName}</span></div>
              <div><span>Email</span><span>{order.customer.email}</span></div>
              <div><span>Method</span><span>Card / wallet</span></div>
              <div className="pay-amount"><span>Amount due</span><span>{order.currency} {order.grandTotal.toLocaleString()}</span></div>
            </div>
            <Link className="text-link" to={`/order/${order.id}`}>Skip to confirmation <ArrowRight size={14} /></Link>
          </aside>
        </div>
      </section>
    </main>
  )
}
