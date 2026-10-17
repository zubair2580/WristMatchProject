import { useQuery } from '@tanstack/react-query'
import { ArrowRight, Heart, Search, SlidersHorizontal, Sparkles } from 'lucide-react'

type Product = { id: string; name: string; price: number; currency: string; colour: string; material: string; imageUrl: string }
type Catalog = { items: Product[]; total: number }

const apiUrl = import.meta.env.VITE_API_URL ?? '/api/v1'

async function loadProducts(): Promise<Catalog> {
  const response = await fetch(`${apiUrl}/products`)
  if (!response.ok) throw new Error('Catalogue unavailable')
  return response.json()
}

export default function ShopperHome() {
  const { data, isPending, isError } = useQuery({ queryKey: ['products'], queryFn: loadProducts })

  return <div className="site-shell"><header className="topbar"><a className="brand" href="/">WRIST<span>MATCH</span></a><nav><a href="#catalog">Collection</a><a href="#how-it-works">How it works</a><a href="#about">About</a></nav><button className="icon-button" aria-label="Search"><Search size={19} /></button></header><main><section className="hero"><div className="hero-copy"><p className="eyebrow"><Sparkles size={15} /> PERSONAL STYLE, VISUALISED</p><h1>Find the watch<br /><em>that feels like you.</em></h1><p className="hero-lede">Explore considered timepieces, then see how they look on your wrist before you decide.</p><a className="primary-button" href="#catalog">Explore the collection <ArrowRight size={18} /></a></div><div className="hero-art"><div className="hero-orbit orbit-one" /><div className="hero-orbit orbit-two" /><div className="hero-watch">⌚</div><div className="hero-note">A more personal<br />way to choose.</div></div></section><section className="intro" id="how-it-works"><p className="eyebrow">THE WRISTMATCH METHOD</p><h2>Less guessing.<br /><em>More knowing.</em></h2><p>Browse by the moments that matter, try a style on your own wrist, and make a choice that feels considered.</p></section><section className="catalog-section" id="catalog"><div className="section-heading"><div><p className="eyebrow">THE COLLECTION</p><h2>Curated for <em>every moment.</em></h2></div><button className="filter-button"><SlidersHorizontal size={17} /> Filters</button></div>{isPending && <div className="status-card">Preparing the collection...</div>}{isError && <div className="status-card error-card">The catalogue is temporarily unavailable. Please check that the API is running and refresh.</div>}{data && <p className="catalog-count">{data.total} considered pieces in the collection</p>}<div className="product-grid">{data?.items.map((product) => <article className="product-card" key={product.id}><div className="product-image"><img src={product.imageUrl} alt={product.name} /><button className="favourite" aria-label={`Save ${product.name}`}><Heart size={17} /></button></div><div className="product-meta"><div><h3>{product.name}</h3><p>{product.colour} · {product.material}</p></div><strong>{product.currency} {product.price.toLocaleString()}</strong></div><a className="text-link" href={`/products/${product.id}`}>View details <ArrowRight size={15} /></a></article>)}</div></section><section className="callout" id="about"><div><p className="eyebrow">YOUR WRIST, YOUR CHOICE</p><h2>See it before<br /><em>you choose it.</em></h2><p>Our image-based preview helps you picture a watch on your own wrist. It is a visual guide, not a measurement or fit guarantee.</p><a className="primary-button light" href="#catalog">Start exploring <ArrowRight size={18} /></a></div><div className="callout-ring">W</div></section></main><footer><a className="brand" href="/">WRIST<span>MATCH</span></a><p>Thoughtful choices, made personal.</p><p>© 2026 WristMatch</p></footer></div>
}
