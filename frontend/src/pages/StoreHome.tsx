import { ArrowRight, ChevronLeft, ChevronRight, Heart, Pause, Play, Sparkles } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { getProducts } from '../store/api'
import { useStore } from '../store/StoreContext'

const slides = [
  { kicker: 'THE EVERYDAY EDIT', title: <>Designed for<br /><em>the everyday.</em></>, copy: 'Understated pieces chosen to move naturally from first coffee to the final meeting.', image: 'https://images.unsplash.com/photo-1533139502658-0198f920d8e8?auto=format&fit=crop&w=1500&q=88', alt: 'Watch photographed outdoors', note: 'Field Olive / Casual edit', position: 'center 58%' },
  { kicker: 'THE EVENING EDIT', title: <>A quieter kind<br /><em>of confidence.</em></>, copy: 'Refined forms, darker tones, and considered details for evenings worth remembering.', image: 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=1500&q=88', alt: 'Dark dial watch', note: 'Noir Atelier / Evening edit', position: 'center 48%' },
  { kicker: 'THE GIFT EDIT', title: <>Something<br /><em>that lasts.</em></>, copy: 'Thoughtful timepieces for the people and milestones that deserve to be marked.', image: 'https://images.unsplash.com/photo-1508057198894-247b23fe5ade?auto=format&fit=crop&w=1500&q=88', alt: 'Classic watch detail', note: 'Champagne Classic / Gift edit', position: 'center 52%' },
]

const promises = [
  { title: 'Private previews', copy: 'Upload once, preview privately, and never share your photo by default.' },
  { title: 'Considered curation', copy: 'A small collection chosen for design, versatility, and everyday ritual.' },
  { title: 'Clear checkout', copy: 'Bank transfer, card, or cash on delivery — pick the path that suits you.' },
]

export default function StoreHome() {
  const [active, setActive] = useState(0)
  const [playing, setPlaying] = useState(true)
  const { isFavourite, toggleFavourite, addToCart, isSelected } = useStore()
  const { data } = useQuery({ queryKey: ['home-products'], queryFn: () => getProducts('?page=1&pageSize=24&sort=recommended') })
  const slide = slides[active]
  const move = (step: number) => setActive(current => (current + step + slides.length) % slides.length)
  useEffect(() => { if (!playing) return; const timer = window.setInterval(() => move(1), 6500); return () => window.clearInterval(timer) }, [playing])

  const featured = useMemo(() => (data?.items ?? []).slice(0, 4), [data])
  const arrivals = useMemo(() => (data?.items ?? []).slice(4, 8), [data])

  return (
    <main>
      <section className="editorial-slider" aria-roledescription="carousel" aria-label="Featured collections">
        <div className="editorial-copy" aria-live="polite">
          <div className="slide-count">0{active + 1}<span>/</span>0{slides.length}</div>
          <p className="eyebrow"><Sparkles size={14} /> {slide.kicker}</p>
          <h1 key={`title-${active}`}>{slide.title}</h1>
          <p className="editorial-lede">{slide.copy}</p>
          <div className="editorial-actions">
            <Link className="primary-button" to="/catalog">Explore the collection <ArrowRight size={17} /></Link>
            <Link className="quiet-link" to="/studio">Try a watch on</Link>
          </div>
          <div className="editorial-controls">
            <button onClick={() => move(-1)} aria-label="Previous slide"><ChevronLeft size={17} /></button>
            <div className="slide-dots">{slides.map((item, index) => <button key={item.kicker} className={index === active ? 'active' : ''} onClick={() => setActive(index)} aria-label={`Show slide ${index + 1}`} aria-current={index === active ? 'true' : undefined}><span /></button>)}</div>
            <button onClick={() => move(1)} aria-label="Next slide"><ChevronRight size={17} /></button>
            <button className="play-toggle" onClick={() => setPlaying(value => !value)} aria-label={playing ? 'Pause slider' : 'Play slider'}>{playing ? <Pause size={14} /> : <Play size={14} />}</button>
          </div>
        </div>
        <div className="editorial-media">
          <img key={slide.image} src={slide.image} alt={slide.alt} style={{ objectPosition: slide.position }} />
          <div className="media-shade" />
          <div className="media-note"><span>FEATURED</span><strong>{slide.note}</strong></div>
          <div className="media-monogram">WM</div>
        </div>
      </section>

      <section className="home-intro">
        <p className="eyebrow">THE WRISTMATCH METHOD</p>
        <div>
          <h2>Less guessing.<br /><em>More knowing.</em></h2>
          <p>Product photographs show you a watch. WristMatch helps you imagine it as yours. Browse by your moments, explore the details, and create a private visual preview when you are ready.</p>
          <Link className="text-link" to="/studio">Discover the studio <ArrowRight size={15} /></Link>
        </div>
      </section>

      <section className="home-featured">
        <div className="home-section-head">
          <div>
            <p className="eyebrow">FEATURED PIECES</p>
            <h2>Chosen for this season.</h2>
          </div>
          <Link className="text-link" to="/catalog">Shop the collection <ArrowRight size={15} /></Link>
        </div>
        <div className="home-product-grid">
          {featured.map(product => (
            <article className="home-product-card" key={product.id}>
              <div className="home-product-image">
                <Link to={`/products/${product.id}`}><img src={product.imageUrl} alt={product.name} /></Link>
                <button className={isFavourite(product.id) ? 'card-favourite saved' : 'card-favourite'} onClick={() => toggleFavourite(product.id)} aria-label={`Favourite ${product.name}`}><Heart size={16} fill={isFavourite(product.id) ? 'currentColor' : 'none'} /></button>
              </div>
              <div className="home-product-body">
                <Link to={`/products/${product.id}`}><h3>{product.name}</h3></Link>
                <p>{product.colour} · {product.material}</p>
                <div className="card-bottom">
                  <strong>{product.currency} {product.price.toLocaleString()}</strong>
                  <button className={isSelected(product.id) ? 'selection-button selected' : 'selection-button'} onClick={() => addToCart(product.id)}>{isSelected(product.id) ? 'In bag' : 'Add to bag'}</button>
                </div>
              </div>
            </article>
          ))}
        </div>
      </section>

      <section className="home-tiles">
        <Link to="/catalog?occasion=Office" className="home-tile tile-sand"><span>01 / FOR YOUR DAY</span><h3>Quiet confidence.</h3><ArrowRight size={18} /></Link>
        <Link to="/catalog?occasion=Wedding" className="home-tile tile-dark"><span>02 / FOR THE MOMENT</span><h3>Worth remembering.</h3><ArrowRight size={18} /></Link>
        <Link to="/catalog?occasion=Gift" className="home-tile tile-gold"><span>03 / FOR SOMEONE SPECIAL</span><h3>Something lasting.</h3><ArrowRight size={18} /></Link>
      </section>

      <section className="home-studio">
        <div className="studio-art"><div className="studio-circle">W</div><div className="studio-play"><Play size={15} fill="currentColor" /></div></div>
        <div>
          <p className="eyebrow">YOUR WRIST, YOUR CHOICE</p>
          <h2>See it before<br /><em>you choose it.</em></h2>
          <p>Upload a photo of your wrist and create a private, image-based visual preview. It is a style guide, not a measurement or fit guarantee.</p>
          <Link className="primary-button light" to="/studio">Enter the studio <ArrowRight size={17} /></Link>
        </div>
      </section>

      {arrivals.length > 0 && (
        <section className="home-featured home-arrivals">
          <div className="home-section-head">
            <div>
              <p className="eyebrow">MORE TO EXPLORE</p>
              <h2>Further into the edit.</h2>
            </div>
            <Link className="text-link" to="/catalog?sort=price-desc">View all pieces <ArrowRight size={15} /></Link>
          </div>
          <div className="home-product-rail">
            {arrivals.map(product => (
              <Link className="home-rail-card" to={`/products/${product.id}`} key={product.id}>
                <img src={product.imageUrl} alt={product.name} />
                <div>
                  <h3>{product.name}</h3>
                  <span>{product.currency} {product.price.toLocaleString()}</span>
                </div>
              </Link>
            ))}
          </div>
        </section>
      )}

      <section className="home-promises">
        {promises.map(item => (
          <article key={item.title}>
            <h3>{item.title}</h3>
            <p>{item.copy}</p>
          </article>
        ))}
      </section>

      <section className="home-bottom">
        <div>
          <p className="eyebrow">A SMALL COLLECTION</p>
          <h2>Made to be<br /><em>noticed slowly.</em></h2>
        </div>
        <div>
          <p>Every piece in our collection is selected for its design, versatility, and the feeling it brings to an everyday ritual.</p>
          <div className="home-bottom-actions">
            <Link className="primary-button" to="/catalog">Browse watches <ArrowRight size={17} /></Link>
            <Link className="text-link" to="/cart">View your bag <ArrowRight size={15} /></Link>
          </div>
        </div>
      </section>
    </main>
  )
}
