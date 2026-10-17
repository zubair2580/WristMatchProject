import type { Product } from './StoreContext'

export type Catalog = { items: Product[]; total: number; page: number; pageSize: number }
export const apiUrl = import.meta.env.VITE_API_URL ?? '/api/v1'

export async function getProducts(params = ''): Promise<Catalog> {
  const response = await fetch(`${apiUrl}/products${params}`)
  if (!response.ok) throw new Error('Catalogue unavailable')
  return response.json()
}

export async function getProduct(id: string): Promise<Product> {
  const response = await fetch(`${apiUrl}/products/${id}`)
  if (!response.ok) throw new Error('Product unavailable')
  return response.json()
}
