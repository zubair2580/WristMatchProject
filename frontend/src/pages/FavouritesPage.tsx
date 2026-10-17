import { ArrowRight, Heart, ShoppingBag, Star } from 'lucide-react'
import { Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { getProducts } from '../store/api'
import { averageRating, colourHex, readReviews } from '../store/productExtras'
import { useStore } from '../store/StoreContext'

export default function FavouritesPage() {
  const { favourites, toggleFavourite, addToCart, isSelected } = useStore()
  const { data } = useQuery({ queryKey: ['favourites-products'], queryFn: () => getProducts('?page=1&pageSize=50') })
  const products = (data?.items ?? []).filter(product => favourites.includes(product.id))

  return (
    <main className="favourites-page favourites-rich">
      <section className="fav-hero">
        <div>
          <p className="eyebrow">FAVOURITES</p>
          <h1>Saved pieces</h1>
        </div>
        <strong>{products.length} saved</strong>
      </section>

      {products.length === 0 ? (
        <div className="empty-results selection-empty">
          <Heart size={25} />
          <h2>No favourites yet</h2>
          <p>Tap the heart on any watch in the collection.</p>
          <Link className="primary-button" to="/catalog">Browse the collection <ArrowRight size={17} /></Link>
        </div>
      ) : (
        <div className="fav-grid">
          {products.map(product => {
            const reviews = readReviews(product.id)
            const rating = averageRating(reviews)
            return (
              <article className="fav-card" key={product.id}>
                <div className="fav-media">
                  <Link to={`/products/${product.id}`}><img src={product.imageUrl} alt={product.name} /></Link>
                  <button className="card-favourite saved" onClick={() => toggleFavourite(product.id)} aria-label={`Remove ${product.name}`}><Heart size={17} fill="currentColor" /></button>
                </div>
                <div className="fav-body">
                  <p className="shop-card-meta"><span className="colour-dot" style={{ background: colourHex(product.colour) }} />{product.colour} · {product.material}</p>
                  <Link to={`/products/${product.id}`}><h3>{product.name}</h3></Link>
                  <div className="shop-rating"><Star size={13} fill={rating ? 'currentColor' : 'none'} /><span>{rating ? rating.toFixed(1) : 'New'}</span><span>({reviews.length})</span></div>
                  <strong>{product.currency} {product.price.toLocaleString()}</strong>
                  <div className="fav-actions">
                    <button className="primary-button compact-button" onClick={() => addToCart(product.id)}>
                      <ShoppingBag size={15} /> {isSelected(product.id) ? 'In bag' : 'Add to bag'}
                    </button>
                    <Link className="text-link" to={`/products/${product.id}`}>View <ArrowRight size={14} /></Link>
                  </div>
                </div>
              </article>
            )
          })}
        </div>
      )}
    </main>
  )
}
