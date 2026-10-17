import { Archive, ChevronLeft, ChevronRight, Pencil, Plus, RotateCcw, Search } from 'lucide-react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { createPortal } from 'react-dom'
import AdminModal from '../components/AdminModal'
import { adminMutation } from '../store/adminApi'
import { apiUrl } from '../store/api'

type AdminProduct = { id: string; name: string; slug: string; description: string; price: number; currency: string; colour: string; material: string; imageUrl: string; retailerUrl: string; occasions: number[]; category: number; categoryId: string | null; categoryName: string | null; isActive: boolean; tryOnReady: boolean; caseDiameterMm: number | null; rowVersion: string }
type CategoryOption = { id: string; name: string; isActive: boolean }
type ProductForm = { name: string; slug: string; description: string; price: number; currency: string; colour: string; material: string; imageUrl: string; retailerUrl: string; category: number; categoryId: string; occasions: number[]; caseDiameterMm: string; rowVersion: string }
const occasionNames: Record<number, string> = { 1: 'Office', 2: 'Wedding', 3: 'Casual', 4: 'Gift' }
const empty: ProductForm = { name: '', slug: '', description: '', price: 0, currency: 'PKR', colour: '', material: '', imageUrl: '/product-placeholder.svg', retailerUrl: 'https://example.com', category: 1, categoryId: '', occasions: [3], caseDiameterMm: '', rowVersion: '' }

export default function AdminProducts() {
  const client = useQueryClient()
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'archived'>('all')
  const [page, setPage] = useState(1)
  const [editing, setEditing] = useState<AdminProduct | null | undefined>(undefined)
  const [form, setForm] = useState<ProductForm>(empty)
  const [files, setFiles] = useState<File[]>([])
  const [source, setSource] = useState('')
  const [licence, setLicence] = useState('')
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)
  const [hoverPreview, setHoverPreview] = useState<{ src: string; name: string; x: number; y: number } | null>(null)
  const pageSize = 8

  const filePreviews = useMemo(() => files.map(file => ({ name: file.name, url: URL.createObjectURL(file) })), [files])
  useEffect(() => () => { filePreviews.forEach(item => URL.revokeObjectURL(item.url)) }, [filePreviews])

  const showThumbPreview = (target: HTMLElement, product: AdminProduct) => {
    const rect = target.getBoundingClientRect()
    const popupWidth = 240
    const popupHeight = 280
    const spaceRight = window.innerWidth - rect.right
    const x = spaceRight > popupWidth + 20 ? rect.right + 14 : Math.max(12, rect.left - popupWidth - 14)
    const y = Math.min(Math.max(12, rect.top + rect.height / 2 - popupHeight / 2), window.innerHeight - popupHeight - 12)
    setHoverPreview({ src: product.imageUrl, name: product.name, x, y })
  }

  const { data = [], isPending, isError } = useQuery<AdminProduct[]>({
    queryKey: ['admin-products'],
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/admin/products`, { credentials: 'include' })
      if (!response.ok) throw new Error('Unable to load products')
      return response.json()
    },
  })
  const categories = useQuery<{ items: CategoryOption[] }>({
    queryKey: ['admin-categories-options'],
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/admin/categories?page=1&pageSize=100`, { credentials: 'include' })
      if (!response.ok) throw new Error('Unable to load categories')
      return response.json()
    },
  })
  const filtered = useMemo(() => data.filter(product => {
    const matchesSearch = `${product.name} ${product.slug} ${product.colour} ${product.material} ${product.categoryName ?? ''}`.toLowerCase().includes(search.toLowerCase())
    const matchesStatus = statusFilter === 'all' || (statusFilter === 'active' ? product.isActive : !product.isActive)
    return matchesSearch && matchesStatus
  }), [data, search, statusFilter])
  const totalPages = Math.max(1, Math.ceil(filtered.length / pageSize))
  const products = filtered.slice((page - 1) * pageSize, page * pageSize)
  const openCreate = () => { setHoverPreview(null); setEditing(null); setForm(empty); setFiles([]); setSource(''); setLicence(''); setError('') }
  const openEdit = (product: AdminProduct) => {
    setHoverPreview(null)
    setEditing(product)
    setForm({ name: product.name, slug: product.slug, description: product.description, price: product.price, currency: product.currency, colour: product.colour, material: product.material, imageUrl: product.imageUrl, retailerUrl: product.retailerUrl, category: product.category, categoryId: product.categoryId ?? '', occasions: product.occasions, caseDiameterMm: product.caseDiameterMm?.toString() ?? '', rowVersion: product.rowVersion })
    setFiles([]); setSource(''); setLicence(''); setError('')
  }
  const close = () => setEditing(undefined)
  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setSaving(true)
    setError('')
    try {
      const payload = JSON.stringify({
        ...form,
        categoryId: form.categoryId || null,
        caseDiameterMm: form.caseDiameterMm ? Number(form.caseDiameterMm) : null,
      })
      const product = editing
        ? await adminMutation<AdminProduct>(`/admin/products/${editing.id}`, 'PUT', payload, { 'Content-Type': 'application/json' })
        : await adminMutation<AdminProduct>('/admin/products', 'POST', payload, { 'Content-Type': 'application/json' })
      if (files.length) {
        if (!source.trim() || !licence.trim()) throw new Error('Image source and licence details are required.')
        for (const [index, file] of files.entries()) {
          const body = new FormData()
          body.append('file', file)
          body.append('source', source)
          body.append('licenceNote', licence)
          body.append('primary', index === 0 ? 'true' : 'false')
          await adminMutation(`/admin/products/${product.id}/assets`, 'POST', body)
        }
      }
      await client.invalidateQueries({ queryKey: ['admin-products'] })
      close()
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Unable to save product.')
    } finally {
      setSaving(false)
    }
  }
  const archive = async (product: AdminProduct) => {
    if (!window.confirm(`Archive ${product.name}?`)) return
    await adminMutation(`/admin/products/${product.id}`, 'DELETE')
    await client.invalidateQueries({ queryKey: ['admin-products'] })
  }
  const reactivate = async (product: AdminProduct) => {
    await adminMutation(`/admin/products/${product.id}/reactivate`, 'POST')
    await client.invalidateQueries({ queryKey: ['admin-products'] })
  }
  const toggleOccasion = (occasion: number) => setForm(value => ({ ...value, occasions: value.occasions.includes(occasion) ? value.occasions.filter(item => item !== occasion) : [...value.occasions, occasion] }))
  const activeCategories = (categories.data?.items ?? []).filter(item => item.isActive)
  const sidePreview = filePreviews[0]?.url || form.imageUrl || '/product-placeholder.svg'

  return (
    <div>
      <div className="page-heading">
        <div>
          <p className="eyebrow">CATALOGUE MANAGEMENT</p>
          <h1>Products</h1>
          <p>Maintain product information, approved imagery, licensing, inventory settings, and try-on readiness.</p>
        </div>
        <button className="primary-button compact-button" onClick={openCreate}><Plus size={17} /> Add product</button>
      </div>
      <section className="panel table-panel">
        <div className="table-toolbar">
          <div className="search-box"><Search size={17} /><input value={search} onChange={event => { setSearch(event.target.value); setPage(1) }} placeholder="Search products, colour or material" /></div>
          <div className="table-actions">
            <select className="outline-button filter-select" value={statusFilter} onChange={event => { setStatusFilter(event.target.value as typeof statusFilter); setPage(1) }}>
              <option value="all">All statuses</option>
              <option value="active">Active only</option>
              <option value="archived">Archived only</option>
            </select>
          </div>
        </div>
        {isPending && <div className="empty-table">Loading catalogue...</div>}
        {isError && <div className="admin-table-error">The protected catalogue could not be loaded.</div>}
        {!isPending && !isError && (
          <>
            <div className="table-scroll">
              <table>
                <thead>
                  <tr><th>Sr #</th><th>Product</th><th>Category</th><th>Price</th><th>Occasions</th><th>Try-on</th><th>Status</th><th /></tr>
                </thead>
                <tbody>
                  {products.map((product, index) => (
                    <tr key={product.id}>
                      <td>{(page - 1) * pageSize + index + 1}</td>
                      <td>
                        <div className="product-cell">
                          <button
                            type="button"
                            className="product-thumb"
                            aria-label={`Preview ${product.name}`}
                            onMouseEnter={event => showThumbPreview(event.currentTarget, product)}
                            onMouseLeave={() => setHoverPreview(null)}
                            onFocus={event => showThumbPreview(event.currentTarget, product)}
                            onBlur={() => setHoverPreview(null)}
                          >
                            <img src={product.imageUrl} alt="" />
                          </button>
                          <div><strong>{product.name}</strong><small>{product.colour} · {product.material}</small></div>
                        </div>
                      </td>
                      <td>{product.categoryName ?? (product.category === 1 ? 'Watch' : 'Bracelet')}</td>
                      <td>{product.currency} {product.price.toLocaleString()}</td>
                      <td><div className="tag-list">{product.occasions.slice(0, 3).map(occasion => <span key={occasion}>{occasionNames[occasion]}</span>)}</div></td>
                      <td><span className={product.tryOnReady ? 'status-pill' : 'status-pill pending'}><i /> {product.tryOnReady ? 'Ready' : 'Review'}</span></td>
                      <td><span className={product.isActive ? 'status-pill' : 'status-pill archived'}><i /> {product.isActive ? 'Active' : 'Archived'}</span></td>
                      <td>
                        <button className="row-action" onClick={() => openEdit(product)} aria-label={`Edit ${product.name}`}><Pencil size={16} /></button>
                        {product.isActive
                          ? <button className="row-action danger" onClick={() => archive(product)} aria-label={`Archive ${product.name}`}><Archive size={16} /></button>
                          : <button className="row-action" onClick={() => reactivate(product)} aria-label={`Reactivate ${product.name}`}><RotateCcw size={16} /></button>}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            {products.length === 0 && <div className="module-empty"><Archive size={22} /><h2>No matching products</h2></div>}
            <div className="table-pagination">
              <span>{filtered.length} products</span>
              <div>
                <button disabled={page === 1} onClick={() => setPage(value => value - 1)}><ChevronLeft size={14} /> Previous</button>
                <span>Page {page} of {totalPages}</span>
                <button disabled={page === totalPages} onClick={() => setPage(value => value + 1)}>Next <ChevronRight size={14} /></button>
              </div>
            </div>
          </>
        )}
      </section>
      {editing !== undefined && (
        <AdminModal
          wide
          eyebrow="CATALOGUE PRODUCT"
          title={editing ? 'Edit product' : 'Add product'}
          onClose={close}
          footer={<><button className="outline-button" onClick={close}>Cancel</button><button form="product-form" className="primary-button compact-button" disabled={saving}>{saving ? 'Saving...' : editing ? 'Save changes' : 'Create product'}</button></>}
        >
          <form id="product-form" className="admin-form product-form-layout" onSubmit={submit}>
            <aside className="product-form-preview">
              <p className="eyebrow">IMAGE PREVIEW</p>
              <div className="product-form-hero">
                <img
                  key={sidePreview}
                  src={sidePreview}
                  alt={form.name || 'Product preview'}
                  onError={event => { event.currentTarget.src = '/product-placeholder.svg' }}
                />
              </div>
              <p className="product-form-caption">{filePreviews[0]?.name || form.name || 'Live preview updates as you edit'}</p>
              <label className="product-image-url">Image URL
                <input value={form.imageUrl} onChange={event => setForm(value => ({ ...value, imageUrl: event.target.value }))} placeholder="/uploads/... or https://..." />
              </label>
              <label className="form-span file-drop product-side-upload">Upload images
                <input type="file" accept="image/jpeg,image/png" multiple onChange={event => setFiles(Array.from(event.target.files ?? []))} />
                <span>{files.length ? `${files.length} image${files.length > 1 ? 's' : ''} selected` : 'JPEG or PNG · Select files'}</span>
              </label>
              {filePreviews.length > 0 && (
                <div className="product-form-thumbs">
                  {filePreviews.map((item, index) => (
                    <button
                      key={item.url}
                      type="button"
                      className={index === 0 ? 'is-active' : undefined}
                      onClick={() => setFiles(current => {
                        const next = [...current]
                        const [picked] = next.splice(index, 1)
                        next.unshift(picked)
                        return next
                      })}
                    >
                      <img src={item.url} alt={item.name} />
                      <span>{item.name}</span>
                    </button>
                  ))}
                </div>
              )}
              {files.length > 0 && (
                <div className="product-licence-fields">
                  <label>Image source<input required value={source} onChange={event => setSource(event.target.value)} /></label>
                  <label>Licence details<input required value={licence} onChange={event => setLicence(event.target.value)} /></label>
                </div>
              )}
            </aside>

            <div className="form-grid">
              <label>Product name<input required value={form.name} onChange={event => setForm(value => ({ ...value, name: event.target.value, slug: value.slug || event.target.value.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '') }))} /></label>
              <label>Slug<input required pattern="[a-z0-9-]+" value={form.slug} onChange={event => setForm(value => ({ ...value, slug: event.target.value.toLowerCase() }))} /></label>
              <label>Price<input required min="0" type="number" value={form.price} onChange={event => setForm(value => ({ ...value, price: Number(event.target.value) }))} /></label>
              <label>Currency<input required maxLength={3} value={form.currency} onChange={event => setForm(value => ({ ...value, currency: event.target.value.toUpperCase() }))} /></label>
              <label>Colour<input required value={form.colour} onChange={event => setForm(value => ({ ...value, colour: event.target.value }))} /></label>
              <label>Material<input required value={form.material} onChange={event => setForm(value => ({ ...value, material: event.target.value }))} /></label>
              <label>Product type<select value={form.category} onChange={event => setForm(value => ({ ...value, category: Number(event.target.value) }))}><option value={1}>Watch</option><option value={2}>Bracelet</option></select></label>
              <label>Catalogue category
                <select value={form.categoryId} onChange={event => setForm(value => ({ ...value, categoryId: event.target.value }))}>
                  <option value="">Unassigned</option>
                  {activeCategories.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}
                </select>
              </label>
              <label>Case diameter (mm)<input min="1" step="0.1" type="number" value={form.caseDiameterMm} onChange={event => setForm(value => ({ ...value, caseDiameterMm: event.target.value }))} /></label>
              <label className="form-span">Description<textarea required rows={4} value={form.description} onChange={event => setForm(value => ({ ...value, description: event.target.value }))} /></label>
              <label className="form-span">Retailer HTTPS URL<input required type="url" value={form.retailerUrl} onChange={event => setForm(value => ({ ...value, retailerUrl: event.target.value }))} /></label>
              <fieldset className="form-span occasion-field"><legend>Occasions</legend>{Object.entries(occasionNames).map(([id, name]) => <label key={id}><input type="checkbox" checked={form.occasions.includes(Number(id))} onChange={() => toggleOccasion(Number(id))} /> {name}</label>)}</fieldset>
            </div>
            {error && <div className="form-error">{error}</div>}
          </form>
        </AdminModal>
      )}

      {hoverPreview && createPortal(
        <div className="product-thumb-popup-fixed" style={{ left: hoverPreview.x, top: hoverPreview.y }} role="tooltip">
          <img src={hoverPreview.src} alt={hoverPreview.name} onError={event => { event.currentTarget.src = '/product-placeholder.svg' }} />
          <em>{hoverPreview.name}</em>
        </div>,
        document.body,
      )}
    </div>
  )
}
