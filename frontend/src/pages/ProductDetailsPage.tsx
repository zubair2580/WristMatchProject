import { ArrowRight, Check, ExternalLink, Heart, Minus, Plus, Share2, Star, ZoomIn } from 'lucide-react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { useEffect, useState, type FormEvent, type MouseEvent } from 'react'
import { getProduct, getProducts } from '../store/api'
import { averageRating, colourOptions, readReviews, relatedByColour, saveReview, type ProductReview } from '../store/productExtras'
import { useStore } from '../store/StoreContext'

export default function ProductDetailsPage() {
  const { id = '' } = useParams()
  const navigate = useNavigate()
  const { data: product, isPending, isError } = useQuery({ queryKey: ['product', id], queryFn: () => getProduct(id) })
  const catalog = useQuery({ queryKey: ['pdp-catalog'], queryFn: () => getProducts('?page=1&pageSize=50&sort=recommended') })
  const { isFavourite, toggleFavourite, addToCart, cart } = useStore()
  const [qty, setQty] = useState(1)
  const [shareNote, setShareNote] = useState('')
  const [zoom, setZoom] = useState(false)
  const [lens, setLens] = useState({ x: 50, y: 50 })
  const [reviews, setReviews] = useState<ProductReview[]>([])
  const [reviewForm, setReviewForm] = useState({ name: '', rating: 5, title: '', body: '' })
  const [reviewNote, setReviewNote] = useState('')

  useEffect(() => {
    if (id) {
      setReviews(readReviews(id))
      setQty(1)
      setReviewNote('')
    }
  }, [id])

  if (isPending) return <main className="detail-page"><div className="status-card">Loading product details...</div></main>
  if (isError || !product) return <main className="detail-page"><div className="status-card error-card">This product is no longer available.</div></main>

  const description = product.description?.trim() || 'A considered timepiece with a clear point of view, designed to become part of the everyday rather than compete with it.'
  const caseDiameter = product.caseDiameterMm ? `${product.caseDiameterMm} mm` : 'Not provided'
  const bagFull = cart.length >= 12 && !cart.some(line => line.id === product.id)
  const rating = averageRating(reviews)
  const colours = colourOptions(catalog.data?.items ?? [], product)
  const related = relatedByColour(catalog.data?.items ?? [], product, 4)

  const onMove = (event: MouseEvent<HTMLDivElement>) => {
    const rect = event.currentTarget.getBoundingClientRect()
    const x = ((event.clientX - rect.left) / rect.width) * 100
    const y = ((event.clientY - rect.top) / rect.height) * 100
    setLens({ x: Math.min(100, Math.max(0, x)), y: Math.min(100, Math.max(0, y)) })
  }

  const share = async () => {
    const url = window.location.href
    try {
      if (navigator.share) await navigator.share({ title: product.name, url })
      else {
        await navigator.clipboard.writeText(url)
        setShareNote('Link copied')
        window.setTimeout(() => setShareNote(''), 2000)
      }
    } catch {
      setShareNote('')
    }
  }

  const submitReview = (event: FormEvent) => {
    event.preventDefault()
    if (!reviewForm.name.trim() || !reviewForm.body.trim()) {
      setReviewNote('Add your name and review text.')
      return
    }
    const next = saveReview(product.id, {
      name: reviewForm.name.trim(),
      rating: reviewForm.rating,
      title: reviewForm.title.trim() || 'Customer review',
      body: reviewForm.body.trim(),
    })
    setReviews(next)
    setReviewForm({ name: '', rating: 5, title: '', body: '' })
    setReviewNote('Thank you — your review is live.')
  }

  return (
    <main className="detail-page shop-pdp">
      <div className="breadcrumbs">
        <Link to="/catalog">Collection</Link><span>/</span>
        <Link to={`/catalog?colour=${encodeURIComponent(product.colour)}`}>{product.colour}</Link><span>/</span>
        <span>{product.name}</span>
      </div>

      <div className="detail-layout shop-pdp-layout">
        <div className="pdp-gallery">
          <div
            className={`pdp-zoom ${zoom ? 'active' : ''}`}
            onMouseEnter={() => setZoom(true)}
            onMouseLeave={() => setZoom(false)}
            onMouseMove={onMove}
          >
            <img src={product.imageUrl} alt={product.name} />
            {zoom && <div className="pdp-zoom-lens" style={{ backgroundImage: `url(${product.imageUrl})`, backgroundPosition: `${lens.x}% ${lens.y}%` }} />}
            <span className="pdp-zoom-hint"><ZoomIn size={14} /> Hover to zoom</span>
          </div>
          <div className="pdp-thumbs">
            <button type="button" className="active"><img src={product.imageUrl} alt="" /></button>
            {related.slice(0, 3).map(item => (
              <button type="button" key={item.id} onClick={() => navigate(`/products/${item.id}`)}>
                <img src={item.imageUrl} alt={item.name} />
              </button>
            ))}
          </div>
        </div>

        <div className="detail-copy shop-pdp-copy">
          <p className="eyebrow">{product.material.toUpperCase()} · {product.occasions[0]?.toUpperCase() || 'WATCH'}</p>
          <h1>{product.name}</h1>
          <div className="shop-rating pdp-rating">
            <Star size={15} fill={rating ? 'currentColor' : 'none'} />
            <strong>{rating ? rating.toFixed(1) : 'New'}</strong>
            <a href="#reviews">{reviews.length} reviews</a>
          </div>
          <p className="detail-price">{product.currency} {product.price.toLocaleString()}</p>
          <p className="detail-description">{description}</p>

          <div className="pdp-swatches">
            <div className="pdp-swatch-label"><span>Colour</span><strong>{product.colour}</strong></div>
            <div className="swatch-row">
              {colours.map(option => (
                <button
                  key={option.colour}
                  type="button"
                  className={option.product.id === product.id ? 'swatch active' : 'swatch'}
                  style={{ background: option.hex }}
                  title={option.colour}
                  onClick={() => option.product.id !== product.id && navigate(`/products/${option.product.id}`)}
                  aria-label={`Colour ${option.colour}`}
                />
              ))}
            </div>
          </div>

          <div className="pdp-buy-row">
            <div className="qty-control large">
              <button aria-label="Decrease quantity" onClick={() => setQty(current => Math.max(1, current - 1))}><Minus size={15} /></button>
              <span>{qty}</span>
              <button aria-label="Increase quantity" onClick={() => setQty(current => Math.min(9, current + 1))}><Plus size={15} /></button>
            </div>
            <button className="primary-button" disabled={bagFull} onClick={() => addToCart(product.id, qty)}>
              Add to bag · {product.currency} {(product.price * qty).toLocaleString()} <ArrowRight size={17} />
            </button>
            <button className="circle-action" onClick={() => toggleFavourite(product.id)} aria-label="Favourite product"><Heart size={18} fill={isFavourite(product.id) ? 'currentColor' : 'none'} /></button>
            <button className="circle-action" onClick={share} aria-label="Share product"><Share2 size={18} /></button>
          </div>
          {shareNote && <p className="detail-share-note">{shareNote}</p>}

          <div className="detail-note"><Check size={17} /><span>Free shipping over PKR 50,000 · Card, bank transfer, or COD at checkout</span></div>

          <dl className="spec-list">
            <div><dt>Colour</dt><dd>{product.colour}</dd></div>
            <div><dt>Material</dt><dd>{product.material}</dd></div>
            <div><dt>Occasions</dt><dd>{product.occasions.join(' · ')}</dd></div>
            <div><dt>Case diameter</dt><dd>{caseDiameter}</dd></div>
          </dl>

          <div className="detail-links">
            <Link className="text-link" to="/studio">Try this watch on your wrist <ArrowRight size={15} /></Link>
            {product.retailerUrl && (
              <a className="text-link" href={product.retailerUrl} target="_blank" rel="noreferrer">
                Visit retailer <ExternalLink size={14} />
              </a>
            )}
          </div>
        </div>
      </div>

      {related.length > 0 && (
        <section className="pdp-related">
          <div className="home-section-head">
            <div>
              <p className="eyebrow">COMPLETE THE LOOK</p>
              <h2>You may also like</h2>
            </div>
            <Link className="text-link" to="/catalog">Shop all <ArrowRight size={15} /></Link>
          </div>
          <div className="home-product-rail">
            {related.map(item => (
              <Link className="home-rail-card" to={`/products/${item.id}`} key={item.id}>
                <img src={item.imageUrl} alt={item.name} />
                <div>
                  <h3>{item.name}</h3>
                  <span>{item.currency} {item.price.toLocaleString()}</span>
                </div>
              </Link>
            ))}
          </div>
        </section>
      )}

      <section className="pdp-reviews" id="reviews">
        <div className="pdp-reviews-head">
          <div>
            <p className="eyebrow">CUSTOMER REVIEWS</p>
            <h2>Rated {rating ? rating.toFixed(1) : '—'} / 5</h2>
            <p>{reviews.length} verified-style reviews from WristMatch shoppers.</p>
          </div>
          <div className="pdp-score">
            <strong>{rating ? rating.toFixed(1) : 'New'}</strong>
            <div>{[1, 2, 3, 4, 5].map(star => <Star key={star} size={16} fill={star <= Math.round(rating) ? 'currentColor' : 'none'} />)}</div>
          </div>
        </div>

        <div className="pdp-reviews-grid">
          <div className="pdp-review-list">
            {reviews.map(review => (
              <article key={review.id} className="pdp-review-card">
                <div className="pdp-review-top">
                  <strong>{review.name}</strong>
                  <span>{new Date(review.createdAt).toLocaleDateString()}</span>
                </div>
                <div className="shop-rating">{[1, 2, 3, 4, 5].map(star => <Star key={star} size={12} fill={star <= review.rating ? 'currentColor' : 'none'} />)}</div>
                <h3>{review.title}</h3>
                <p>{review.body}</p>
              </article>
            ))}
          </div>

          <form className="pdp-review-form" onSubmit={submitReview}>
            <p className="eyebrow">WRITE A REVIEW</p>
            <h3>Share your experience</h3>
            <label>Name<input required value={reviewForm.name} onChange={event => setReviewForm(current => ({ ...current, name: event.target.value }))} placeholder="Your name" /></label>
            <label>Rating
              <select value={reviewForm.rating} onChange={event => setReviewForm(current => ({ ...current, rating: Number(event.target.value) }))}>
                {[5, 4, 3, 2, 1].map(value => <option key={value} value={value}>{value} stars</option>)}
              </select>
            </label>
            <label>Title<input value={reviewForm.title} onChange={event => setReviewForm(current => ({ ...current, title: event.target.value }))} placeholder="Sum up your review" /></label>
            <label>Review<textarea required rows={4} value={reviewForm.body} onChange={event => setReviewForm(current => ({ ...current, body: event.target.value }))} placeholder="How does it wear? Would you recommend it?" /></label>
            {reviewNote && <p className="detail-share-note">{reviewNote}</p>}
            <button className="primary-button" type="submit">Submit review <ArrowRight size={16} /></button>
          </form>
        </div>
      </section>
    </main>
  )
}
