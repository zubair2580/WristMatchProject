import { ArrowRight, GitCompare, Plus, Star, X } from 'lucide-react'
import { Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { getProducts } from '../store/api'
import { averageRating, colourHex, readReviews } from '../store/productExtras'
import { useStore } from '../store/StoreContext'

export default function ComparePage() {
  const { selection, removeFromCart, addToCart } = useStore()
  const { data } = useQuery({ queryKey: ['compare-products'], queryFn: () => getProducts('?page=1&pageSize=50') })
  const products = (data?.items ?? []).filter(product => selection.includes(product.id)).slice(0, 3)
  const slots = [0, 1, 2]

  return (
    <main className="compare-page compare-rich">
      <section className="compare-hero">
        <div>
          <p className="eyebrow">COMPARE</p>
          <h1>Side by side</h1>
        </div>
        <p className="compare-count">{products.length} of 3 selected</p>
      </section>

      {products.length === 0 ? (
        <div className="empty-results selection-empty">
          <GitCompare size={25} />
          <h2>Nothing to compare yet</h2>
          <p>Add watches from the collection, then return here.</p>
          <Link className="primary-button" to="/catalog">Browse watches <ArrowRight size={17} /></Link>
        </div>
      ) : (
        <>
          <div className="compare-board">
            {slots.map(index => {
              const product = products[index]
              if (!product) {
                return (
                  <Link key={`empty-${index}`} className="compare-slot empty" to="/catalog">
                    <Plus size={22} />
                    <span>Add a watch</span>
                  </Link>
                )
              }
              const reviews = readReviews(product.id)
              const rating = averageRating(reviews)
              return (
                <article className="compare-slot" key={product.id}>
                  <button className="compare-remove" onClick={() => removeFromCart(product.id)} aria-label={`Remove ${product.name}`}><X size={16} /></button>
                  <Link to={`/products/${product.id}`} className="compare-media"><img src={product.imageUrl} alt={product.name} /></Link>
                  <p className="eyebrow">{product.material}</p>
                  <h2><Link to={`/products/${product.id}`}>{product.name}</Link></h2>
                  <strong>{product.currency} {product.price.toLocaleString()}</strong>
                  <div className="shop-rating"><Star size={13} fill={rating ? 'currentColor' : 'none'} /><span>{rating ? rating.toFixed(1) : 'New'}</span></div>
                  <dl className="compare-specs">
                    <div><dt>Colour</dt><dd><i className="colour-dot" style={{ background: colourHex(product.colour) }} />{product.colour}</dd></div>
                    <div><dt>Material</dt><dd>{product.material}</dd></div>
                    <div><dt>Occasions</dt><dd>{product.occasions.join(' · ')}</dd></div>
                    <div><dt>Case</dt><dd>{product.caseDiameterMm ? `${product.caseDiameterMm} mm` : '—'}</dd></div>
                  </dl>
                  <div className="compare-actions">
                    <button className="primary-button compact-button" onClick={() => addToCart(product.id)}>Add to bag</button>
                    <Link className="text-link" to={`/products/${product.id}`}>Details <ArrowRight size={14} /></Link>
                  </div>
                </article>
              )
            })}
          </div>

          <section className="compare-matrix">
            <h2>At a glance</h2>
            <table>
              <thead>
                <tr>
                  <th>Detail</th>
                  {products.map(product => <th key={product.id}>{product.name}</th>)}
                </tr>
              </thead>
              <tbody>
                <tr><th>Price</th>{products.map(p => <td key={p.id}>{p.currency} {p.price.toLocaleString()}</td>)}</tr>
                <tr><th>Colour</th>{products.map(p => <td key={p.id}>{p.colour}</td>)}</tr>
                <tr><th>Material</th>{products.map(p => <td key={p.id}>{p.material}</td>)}</tr>
                <tr><th>Occasions</th>{products.map(p => <td key={p.id}>{p.occasions.join(', ')}</td>)}</tr>
              </tbody>
            </table>
          </section>
        </>
      )}
    </main>
  )
}
