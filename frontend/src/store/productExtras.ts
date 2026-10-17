import type { Product } from './StoreContext'

export type ProductReview = {
  id: string
  productId: string
  name: string
  rating: number
  title: string
  body: string
  createdAt: string
}

const COLOUR_HEX: Record<string, string> = {
  black: '#1f1c1a',
  silver: '#c5c0b8',
  gold: '#c4a46a',
  blue: '#3d5470',
  navy: '#243447',
  green: '#4f6a52',
  olive: '#6b6a3d',
  brown: '#6b4a32',
  champagne: '#d6c3a1',
  white: '#f4f0e8',
  grey: '#7a756e',
  gray: '#7a756e',
  red: '#8b3a3a',
}

const SEED_REVIEWS: Omit<ProductReview, 'id' | 'createdAt'>[] = [
  { productId: '*', name: 'Ayesha K.', rating: 5, title: 'Quiet luxury', body: 'The proportions feel considered and the strap sits comfortably through a full workday.' },
  { productId: '*', name: 'Hassan R.', rating: 4, title: 'Looks better in person', body: 'Colour is richer than the photo suggested. Shipping was quick across Karachi.' },
  { productId: '*', name: 'Mariam S.', rating: 5, title: 'Gift that landed', body: 'Bought this as a gift — packaging felt thoughtful and the watch matched the occasion.' },
  { productId: '*', name: 'Bilal A.', rating: 4, title: 'Try-on helped', body: 'Used the studio preview first. It is not a fit guarantee, but it made choosing easier.' },
]

export function colourHex(colour: string) {
  const key = colour.trim().toLowerCase()
  if (COLOUR_HEX[key]) return COLOUR_HEX[key]
  const hit = Object.keys(COLOUR_HEX).find(item => key.includes(item))
  return hit ? COLOUR_HEX[hit] : '#b28a52'
}

function storageKey(productId: string) {
  return `wristmatch.reviews.${productId}`
}

export function readReviews(productId: string): ProductReview[] {
  try {
    const saved = JSON.parse(localStorage.getItem(storageKey(productId)) ?? '[]') as ProductReview[]
    if (Array.isArray(saved) && saved.length) return saved.sort((a, b) => b.createdAt.localeCompare(a.createdAt))
  } catch { /* ignore */ }
  const day = 86400000
  return SEED_REVIEWS.map((item, index) => ({
    ...item,
    id: `seed-${productId}-${index}`,
    productId,
    createdAt: new Date(Date.now() - (index + 1) * day * 3).toISOString(),
  }))
}

export function saveReview(productId: string, review: Omit<ProductReview, 'id' | 'productId' | 'createdAt'>) {
  const current = readReviews(productId).filter(item => !item.id.startsWith('seed-'))
  const next: ProductReview = {
    ...review,
    id: crypto.randomUUID(),
    productId,
    createdAt: new Date().toISOString(),
  }
  const merged = [next, ...current]
  localStorage.setItem(storageKey(productId), JSON.stringify(merged))
  return readReviews(productId)
}

export function averageRating(reviews: ProductReview[]) {
  if (!reviews.length) return 0
  return reviews.reduce((sum, item) => sum + item.rating, 0) / reviews.length
}

export function relatedByColour(products: Product[], current: Product, limit = 6) {
  const sameColour = products.filter(item => item.id !== current.id && item.colour.toLowerCase() === current.colour.toLowerCase())
  const others = products.filter(item => item.id !== current.id && !sameColour.some(match => match.id === item.id))
  return [...sameColour, ...others].slice(0, limit)
}

export function colourOptions(products: Product[], current: Product) {
  const map = new Map<string, Product>()
  map.set(current.colour, current)
  for (const product of products) {
    if (!map.has(product.colour)) map.set(product.colour, product)
  }
  return [...map.entries()].map(([colour, product]) => ({ colour, product, hex: colourHex(colour) }))
}
