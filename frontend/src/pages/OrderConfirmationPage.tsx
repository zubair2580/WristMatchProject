import { ArrowRight, Check, Copy, Package } from 'lucide-react'
import { useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { getCheckoutOrder } from '../store/checkoutApi'

export default function OrderConfirmationPage() {
  const { id = '' } = useParams()
  const [copied, setCopied] = useState(false)
  const { data: order, isPending, isError } = useQuery({
    queryKey: ['checkout-order', id],
    queryFn: () => getCheckoutOrder(id),
    enabled: Boolean(id),
  })

  const copyIban = async () => {
    if (!order?.bankInstructions) return
    await navigator.clipboard.writeText(order.bankInstructions.iban)
    setCopied(true)
  }

  if (isPending) return <main className="order-confirm-page"><div className="status-card">Loading confirmation...</div></main>
  if (isError || !order) {
    return (
      <main className="order-confirm-page">
        <div className="status-card error-card">Order not found. <Link to="/account">Open your account</Link></div>
      </main>
    )
  }

  return (
    <main className="order-confirm-page">
      <section className="order-confirm-card">
        <div className="checkout-success-mark"><Check size={28} /></div>
        <p className="eyebrow">BOOKING CONFIRMED</p>
        <h1>Thank you, {order.customer.firstName}</h1>
        <p className="order-confirm-lede">Your order <strong>{order.orderNumber}</strong> is saved and visible in the admin orders desk.</p>

        <div className="order-confirm-meta">
          <div><span>Status</span><strong>{order.status}</strong></div>
          <div><span>Payment</span><strong>{order.paymentStatus}</strong></div>
          <div><span>Fulfilment</span><strong>{order.fulfilmentStatus}</strong></div>
          <div><span>Total</span><strong>{order.currency} {order.grandTotal.toLocaleString()}</strong></div>
        </div>

        {order.shippingAddress && (
          <div className="order-confirm-block">
            <p className="eyebrow">DELIVER TO</p>
            <strong>{order.shippingAddress.recipientName}</strong>
            <p>{order.shippingAddress.addressLine1}{order.shippingAddress.addressLine2 ? `, ${order.shippingAddress.addressLine2}` : ''}</p>
            <p>{order.shippingAddress.city}{order.shippingAddress.postalCode ? ` ${order.shippingAddress.postalCode}` : ''}</p>
            <p>{order.shippingAddress.phone}</p>
          </div>
        )}

        {order.bankInstructions && (
          <div className="order-confirm-block bank">
            <p className="eyebrow">BANK TRANSFER</p>
            <p><strong>{order.bankInstructions.bankName}</strong></p>
            <p>{order.bankInstructions.accountTitle}</p>
            <p className="iban-row">
              <span>{order.bankInstructions.iban}</span>
              <button type="button" onClick={copyIban}><Copy size={14} /> {copied ? 'Copied' : 'Copy'}</button>
            </p>
            <p>Reference: <strong>{order.bankInstructions.reference}</strong></p>
          </div>
        )}

        {order.codNote && (
          <div className="order-confirm-block">
            <p className="eyebrow">CASH ON DELIVERY</p>
            <p>{order.codNote}</p>
          </div>
        )}

        {order.paymentMethod === 'card' && order.paymentStatus !== 'Paid' && (
          <Link className="primary-button" to={`/pay/${order.id}`}>Complete card payment <ArrowRight size={16} /></Link>
        )}

        <div className="order-confirm-actions">
          <Link className="primary-button" to="/account"><Package size={16} /> View my orders</Link>
          <Link className="text-link" to="/catalog">Continue shopping <ArrowRight size={14} /></Link>
        </div>
      </section>
    </main>
  )
}
