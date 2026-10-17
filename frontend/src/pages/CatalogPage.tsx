import { Check, ChevronDown, Grid2x2, Heart, LayoutGrid, RotateCcw, Search, SlidersHorizontal, Star, X } from 'lucide-react'
import { useEffect, useMemo, useRef, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { getProducts } from '../store/api'
import { averageRating, colourHex, readReviews } from '../store/productExtras'
import { useStore } from '../store/StoreContext'

const COLOURS = ['Black', 'Silver', 'Gold', 'Blue', 'Green', 'Brown', 'Navy']
const MATERIALS = ['Leather', 'Stainless Steel', 'NATO', 'Rubber', 'Titanium', 'Canvas']
const SORT_OPTIONS = [
  { value: 'recommended', label: 'Featured' },
  { value: 'price-asc', label: 'Price, low to high' },
  { value: 'price-desc', label: 'Price, high to low' },
  { value: 'name', label: 'Alphabetically, A–Z' },
]

export default function CatalogPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const [search, setSearch] = useState(searchParams.get('search') ?? '')
  const [budget, setBudget] = useState(searchParams.get('maxBudget') ?? '')
  const [minBudget, setMinBudget] = useState(searchParams.get('minBudget') ?? '')
  const [occasion, setOccasion] = useState(searchParams.get('occasion') ?? '')
  const [colour, setColour] = useState(searchParams.get('colour') ?? '')
  const [material, setMaterial] = useState(searchParams.get('material') ?? '')
  const [sort, setSort] = useState(searchParams.get('sort') ?? 'recommended')
  const [page, setPage] = useState(Number(searchParams.get('page') ?? '1') || 1)
  const [dense, setDense] = useState(false)
  const [sortOpen, setSortOpen] = useState(false)
  const sortRef = useRef<HTMLDivElement>(null)
  const { isFavourite, toggleFavourite, isSelected, addToCart } = useStore()

  useEffect(() => {
    const onClick = (event: MouseEvent) => {
      if (!sortRef.current?.contains(event.target as Node)) setSortOpen(false)
    }
    window.addEventListener('mousedown', onClick)
    return () => window.removeEventListener('mousedown', onClick)
  }, [])

  useEffect(() => {
    const next = new URLSearchParams()
    if (search) next.set('search', search)
    if (budget) next.set('maxBudget', budget)
    if (minBudget) next.set('minBudget', minBudget)
    if (occasion) next.set('occasion', occasion)
    if (colour) next.set('colour', colour)
    if (material) next.set('material', material)
    if (sort !== 'recommended') next.set('sort', sort)
    if (page > 1) next.set('page', String(page))
    setSearchParams(next, { replace: true })
  }, [search, budget, minBudget, occasion, colour, material, sort, page, setSearchParams])

  const query = useMemo(() => {
    const params = new URLSearchParams({ sort, page: String(page), pageSize: dense ? '16' : '12' })
    if (search.trim()) params.set('search', search.trim())
    if (budget) params.set('maxBudget', budget)
    if (minBudget) params.set('minBudget', minBudget)
    if (occasion) params.set('occasion', occasion)
    if (colour) params.set('colour', colour)
    if (material) params.set('material', material)
    return `?${params.toString()}`
  }, [search, budget, minBudget, occasion, colour, material, sort, page, dense])

  const { data, isPending, isError } = useQuery({ queryKey: ['catalog', query], queryFn: () => getProducts(query) })

  const activeFilters = [
    search && { key: 'search', label: `"${search}"` },
    minBudget && { key: 'minBudget', label: `From PKR ${Number(minBudget).toLocaleString()}` },
    budget && { key: 'budget', label: `Up to PKR ${Number(budget).toLocaleString()}` },
    occasion && { key: 'occasion', label: occasion },
    colour && { key: 'colour', label: colour },
    material && { key: 'material', label: material },
  ].filter(Boolean) as { key: string; label: string }[]

  const reset = () => {
    setSearch(''); setBudget(''); setMinBudget(''); setOccasion(''); setColour(''); setMaterial(''); setSort('recommended'); setPage(1)
  }

  const clearChip = (key: string) => {
    if (key === 'search') setSearch('')
    if (key === 'budget') setBudget('')
    if (key === 'minBudget') setMinBudget('')
    if (key === 'occasion') setOccasion('')
    if (key === 'colour') setColour('')
    if (key === 'material') setMaterial('')
    setPage(1)
  }

  const totalPages = Math.max(1, Math.ceil((data?.total ?? 0) / (data?.pageSize ?? 12)))

  return (
    <main className="catalog-page shop-collection">
      <div className="shop-collection-hero">
        <div>
          <p className="eyebrow">COLLECTION</p>
          <h1>Shop all watches</h1>
        </div>
        <div className="shop-collection-stats">
          <div><strong>{data?.total ?? '—'}</strong><span>pieces</span></div>
          <div><strong>{COLOURS.length}</strong><span>colour ways</span></div>
          <div><strong>3</strong><span>pay options</span></div>
        </div>
      </div>

      <div className="catalog-layout shop-layout">
        <aside className="catalog-filters shop-filters">
          <div className="filter-title"><span>Filters</span><SlidersHorizontal size={17} /></div>
          <label className="filter-search">Search
            <div className="filter-search-box">
              <Search size={16} />
              <input value={search} onChange={event => { setSearch(event.target.value); setPage(1) }} placeholder="Search watches..." />
            </div>
          </label>
          <label>Price from
            <select value={minBudget} onChange={event => { setMinBudget(event.target.value); setPage(1) }}>
              <option value="">No minimum</option>
              <option value="10000">PKR 10,000+</option>
              <option value="15000">PKR 15,000+</option>
              <option value="20000">PKR 20,000+</option>
              <option value="30000">PKR 30,000+</option>
            </select>
          </label>
          <label>Price up to
            <select value={budget} onChange={event => { setBudget(event.target.value); setPage(1) }}>
              <option value="">Any budget</option>
              <option value="15000">Under PKR 15,000</option>
              <option value="20000">Under PKR 20,000</option>
              <option value="30000">Under PKR 30,000</option>
              <option value="50000">Under PKR 50,000</option>
            </select>
          </label>
          <label>Occasion
            <select value={occasion} onChange={event => { setOccasion(event.target.value); setPage(1) }}>
              <option value="">All occasions</option>
              <option value="Office">Office</option>
              <option value="Wedding">Wedding</option>
              <option value="Casual">Casual</option>
              <option value="Gift">Gift</option>
            </select>
          </label>
          <fieldset className="filter-chips colour-swatch-filter">
            <legend>Colour</legend>
            <div>
              {COLOURS.map(item => (
                <button type="button" key={item} className={colour === item ? 'active' : ''} onClick={() => { setColour(current => current === item ? '' : item); setPage(1) }} title={item}>
                  <i style={{ background: colourHex(item) }} />
                  <span>{item}</span>
                </button>
              ))}
            </div>
          </fieldset>
          <fieldset className="filter-chips">
            <legend>Material</legend>
            <div>
              {MATERIALS.map(item => (
                <button type="button" key={item} className={material === item ? 'active' : ''} onClick={() => { setMaterial(current => current === item ? '' : item); setPage(1) }}>{item}</button>
              ))}
            </div>
          </fieldset>
          <button className="filter-reset" onClick={reset}><RotateCcw size={14} /> Clear all</button>
        </aside>

        <section className="catalog-results">
          <div className="shop-toolbar">
            <div>
              <strong>{data?.total ?? 0} products</strong>
              {activeFilters.length > 0 && <span> · {activeFilters.length} filters</span>}
            </div>
            <div className="shop-toolbar-actions">
              <div className="view-toggle">
                <button type="button" className={!dense ? 'active' : ''} onClick={() => setDense(false)} aria-label="Comfortable grid"><LayoutGrid size={16} /></button>
                <button type="button" className={dense ? 'active' : ''} onClick={() => setDense(true)} aria-label="Dense grid"><Grid2x2 size={16} /></button>
              </div>
              <div className="sort-menu" ref={sortRef}>
                <button type="button" className="sort-trigger" onClick={() => setSortOpen(value => !value)} aria-expanded={sortOpen}>
                  <span>Sort</span>
                  <strong>{SORT_OPTIONS.find(item => item.value === sort)?.label ?? 'Featured'}</strong>
                  <ChevronDown size={16} />
                </button>
                {sortOpen && (
                  <ul className="sort-list" role="listbox">
                    {SORT_OPTIONS.map(option => (
                      <li key={option.value}>
                        <button
                          type="button"
                          className={sort === option.value ? 'active' : ''}
                          onClick={() => { setSort(option.value); setPage(1); setSortOpen(false) }}
                        >
                          {option.label}
                          {sort === option.value && <Check size={15} />}
                        </button>
                      </li>
                    ))}
                  </ul>
                )}
              </div>
            </div>
          </div>

          {activeFilters.length > 0 && (
            <div className="active-filter-row">
              {activeFilters.map(item => (
                <button key={item.key} type="button" onClick={() => clearChip(item.key)}>{item.label} <X size={13} /></button>
              ))}
            </div>
          )}

          {isPending && <div className="status-card">Loading products...</div>}
          {isError && <div className="status-card error-card">Unable to load the collection.</div>}
          {!isPending && !isError && data?.items.length === 0 && (
            <div className="empty-results">
              <h2>No products found</h2>
              <p>Try removing a filter or searching a different term.</p>
              <button className="primary-button" onClick={reset}>Reset filters</button>
            </div>
          )}

          <div className={dense ? 'catalog-grid shop-grid dense' : 'catalog-grid shop-grid'}>
            {data?.items.map(product => {
              const reviews = readReviews(product.id)
              const rating = averageRating(reviews)
              return (
                <article className="catalog-card shop-card" key={product.id}>
                  <div className="catalog-card-image">
                    <Link to={`/products/${product.id}`}><img src={product.imageUrl} alt={product.name} /></Link>
                    <button onClick={() => toggleFavourite(product.id)} className={isFavourite(product.id) ? 'card-favourite saved' : 'card-favourite'} aria-label={`Favourite ${product.name}`}><Heart size={17} fill={isFavourite(product.id) ? 'currentColor' : 'none'} /></button>
                    <button className="quick-add" onClick={() => addToCart(product.id)}>{isSelected(product.id) ? 'Update bag' : 'Quick add'}</button>
                  </div>
                  <div className="catalog-card-body">
                    <div className="shop-card-meta">
                      <span className="colour-dot" style={{ background: colourHex(product.colour) }} title={product.colour} />
                      <span>{product.colour}</span>
                      <span>·</span>
                      <span>{product.material}</span>
                    </div>
                    <Link to={`/products/${product.id}`}><h3>{product.name}</h3></Link>
                    <div className="shop-rating">
                      <Star size={13} fill={rating ? 'currentColor' : 'none'} />
                      <span>{rating ? rating.toFixed(1) : 'New'}</span>
                      <span>({reviews.length})</span>
                    </div>
                    <div className="card-bottom">
                      <strong>{product.currency} {product.price.toLocaleString()}</strong>
                      <Link className="selection-button" to={`/products/${product.id}`}>View</Link>
                    </div>
                  </div>
                </article>
              )
            })}
          </div>

          {totalPages > 1 && (
            <div className="catalog-pagination">
              <button disabled={page <= 1} onClick={() => setPage(current => Math.max(1, current - 1))}>Previous</button>
              <span>Page {page} of {totalPages}</span>
              <button disabled={page >= totalPages} onClick={() => setPage(current => Math.min(totalPages, current + 1))}>Next</button>
            </div>
          )}
        </section>
      </div>
    </main>
  )
}
