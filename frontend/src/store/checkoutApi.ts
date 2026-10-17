import { apiUrl } from './api'
import type { PaymentMethod } from './StoreContext'

export type CheckoutAddress = {
  recipientName: string
  phone: string
  addressLine1: string
  addressLine2?: string
  city: string
  stateProvince?: string
  postalCode?: string
  countryCode?: string
}

export type CheckoutOrder = {
  id: string
  orderNumber: string
  status: string
  paymentStatus: string
  fulfilmentStatus: string
  paymentMethod: PaymentMethod
  currency: string
  subtotal: number
  shipping: number
  taxTotal: number
  grandTotal: number
  placedAt: string
  confirmed: boolean
  customer: { id: string; customerNumber: string; firstName: string; lastName: string; email: string; phone: string }
  shippingAddress: CheckoutAddress | null
  paymentRedirectUrl: string | null
  bankInstructions: { bankName: string; accountTitle: string; iban: string; reference: string } | null
  codNote: string | null
}

export type CustomerAccount = {
  id: string
  customerNumber: string
  firstName: string
  lastName: string
  email: string
  phone: string
  hasPassword: boolean
}

export type CustomerOrderSummary = {
  id: string
  orderNumber: string
  status: string
  paymentStatus: string
  fulfilmentStatus: string
  currency: string
  grandTotal: number
  placedAt: string
  itemCount: number
}

async function readError(response: Response) {
  const problem = await response.json().catch(() => null) as { detail?: string } | null
  return problem?.detail ?? 'Request failed.'
}

export async function placeCheckoutOrder(body: {
  firstName: string
  lastName: string
  email: string
  phone: string
  paymentMethod: PaymentMethod
  notes?: string
  shippingAddress: CheckoutAddress
  lines: { productId: string; quantity: number }[]
  createAccount?: boolean
  password?: string
}): Promise<CheckoutOrder> {
  const response = await fetch(`${apiUrl}/checkout/orders`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  if (!response.ok) throw new Error(await readError(response))
  return response.json()
}

export async function getCheckoutOrder(id: string): Promise<CheckoutOrder> {
  const response = await fetch(`${apiUrl}/checkout/orders/${id}`, { credentials: 'include' })
  if (!response.ok) throw new Error(await readError(response))
  return response.json()
}

export async function completeCheckoutPayment(id: string, success = true): Promise<CheckoutOrder> {
  const response = await fetch(`${apiUrl}/checkout/orders/${id}/pay`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ success }),
  })
  if (!response.ok) throw new Error(await readError(response))
  return response.json()
}

export async function registerCustomer(body: { firstName: string; lastName: string; email: string; phone: string; password: string }): Promise<CustomerAccount> {
  const response = await fetch(`${apiUrl}/checkout/account/register`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  if (!response.ok) throw new Error(await readError(response))
  return response.json()
}

export async function loginCustomer(email: string, password: string): Promise<CustomerAccount> {
  const response = await fetch(`${apiUrl}/checkout/account/login`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  })
  if (!response.ok) throw new Error(await readError(response))
  return response.json()
}

export async function logoutCustomer() {
  await fetch(`${apiUrl}/checkout/account/logout`, { method: 'POST', credentials: 'include' })
}

export async function getCustomerMe(): Promise<CustomerAccount | null> {
  const response = await fetch(`${apiUrl}/checkout/account/me`, { credentials: 'include' })
  if (response.status === 401) return null
  if (!response.ok) throw new Error(await readError(response))
  return response.json()
}

export async function getCustomerOrders(): Promise<CustomerOrderSummary[]> {
  const response = await fetch(`${apiUrl}/checkout/account/orders`, { credentials: 'include' })
  if (!response.ok) throw new Error(await readError(response))
  const data = await response.json() as { items: CustomerOrderSummary[] }
  return data.items
}
