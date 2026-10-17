import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'

export type Product = { id: string; name: string; slug: string; description?: string; price: number; currency: string; colour: string; material: string; imageUrl: string; retailerUrl?: string; caseDiameterMm?: number | null; occasions: string[] }
export type CartLine = { id: string; quantity: number }
export type PaymentMethod = 'card' | 'bank' | 'cod'

type StoreContextValue = {
  favourites: string[]
  cart: CartLine[]
  selection: string[]
  paymentMethod: PaymentMethod
  setPaymentMethod: (method: PaymentMethod) => void
  toggleFavourite: (id: string) => void
  addToCart: (id: string, quantity?: number) => void
  setCartQuantity: (id: string, quantity: number) => void
  removeFromCart: (id: string) => void
  clearCart: () => void
  toggleSelection: (id: string) => void
  isFavourite: (id: string) => boolean
  isSelected: (id: string) => boolean
  cartCount: number
}

const StoreContext = createContext<StoreContextValue | null>(null)
const MAX_CART_LINES = 12

function readIds(key: string): string[] {
  try { return JSON.parse(localStorage.getItem(key) ?? '[]') as string[] } catch { return [] }
}

function readCart(key: string): CartLine[] {
  try {
    const raw = JSON.parse(localStorage.getItem(key) ?? 'null')
    if (Array.isArray(raw) && raw.every(item => typeof item === 'object' && item && 'id' in item)) {
      return (raw as CartLine[]).filter(item => item.quantity > 0).slice(0, MAX_CART_LINES)
    }
    if (Array.isArray(raw) && raw.every(item => typeof item === 'string')) {
      return (raw as string[]).slice(0, MAX_CART_LINES).map(id => ({ id, quantity: 1 }))
    }
  } catch { /* ignore */ }
  return []
}

export function StoreProvider({ children }: { children: ReactNode }) {
  const [favourites, setFavourites] = useState<string[]>(() => readIds('wristmatch.favourites'))
  const [cart, setCart] = useState<CartLine[]>(() => readCart('wristmatch.selection'))
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>(() => {
    const saved = localStorage.getItem('wristmatch.payment')
    return saved === 'bank' || saved === 'cod' || saved === 'card' ? saved : 'card'
  })

  useEffect(() => localStorage.setItem('wristmatch.favourites', JSON.stringify(favourites)), [favourites])
  useEffect(() => localStorage.setItem('wristmatch.selection', JSON.stringify(cart)), [cart])
  useEffect(() => localStorage.setItem('wristmatch.payment', paymentMethod), [paymentMethod])

  const value = useMemo<StoreContextValue>(() => ({
    favourites,
    cart,
    selection: cart.map(line => line.id),
    paymentMethod,
    setPaymentMethod,
    toggleFavourite: (id: string) => setFavourites(current => current.includes(id) ? current.filter(item => item !== id) : [...current, id]),
    addToCart: (id: string, quantity = 1) => setCart(current => {
      const existing = current.find(line => line.id === id)
      if (existing) {
        return current.map(line => line.id === id ? { ...line, quantity: Math.min(9, line.quantity + quantity) } : line)
      }
      if (current.length >= MAX_CART_LINES) return current
      return [...current, { id, quantity: Math.max(1, Math.min(9, quantity)) }]
    }),
    setCartQuantity: (id: string, quantity: number) => setCart(current => {
      if (quantity <= 0) return current.filter(line => line.id !== id)
      return current.map(line => line.id === id ? { ...line, quantity: Math.min(9, quantity) } : line)
    }),
    removeFromCart: (id: string) => setCart(current => current.filter(line => line.id !== id)),
    clearCart: () => setCart([]),
    toggleSelection: (id: string) => setCart(current => {
      if (current.some(line => line.id === id)) return current.filter(line => line.id !== id)
      if (current.length >= MAX_CART_LINES) return current
      return [...current, { id, quantity: 1 }]
    }),
    isFavourite: (id: string) => favourites.includes(id),
    isSelected: (id: string) => cart.some(line => line.id === id),
    cartCount: cart.reduce((sum, line) => sum + line.quantity, 0),
  }), [favourites, cart, paymentMethod])

  return <StoreContext.Provider value={value}>{children}</StoreContext.Provider>
}

export function useStore() {
  const context = useContext(StoreContext)
  if (!context) throw new Error('StoreProvider is required')
  return context
}
