import { ArrowRight, Heart, Mail, Menu, MessageCircle, Minus, Phone, Plus, Send, ShoppingBag, Trash2, X } from 'lucide-react'
import { useEffect, useMemo, useRef, useState, type FormEvent } from 'react'
import { NavLink, Outlet, Link, useLocation } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { getProducts } from '../store/api'
import { colourHex } from '../store/productExtras'
import { useStore } from '../store/StoreContext'

const WHATSAPP = '923001234567'
const PHONE_DISPLAY = '+92 300 123 4567'
const PHONE_TEL = '+923001234567'
const EMAIL = 'hello@wristmatch.local'

type ChatMessage = { role: 'bot' | 'user'; text: string }

function replyTo(message: string) {
  const text = message.toLowerCase()
  if (text.includes('look') || text.includes('skin') || text.includes('match'))
    return 'Open Look Match for a face selfie. YouCam Skin Analysis helps rank metals and colours that suit you, then jump into Studio to try a watch on your wrist.'
  if (text.includes('try') || text.includes('studio') || text.includes('preview') || text.includes('youcam'))
    return 'Open Look Match or Try on. Studio uses YouCam Watch Virtual Try-On with a private wrist photo. Previews stay private and expire automatically.'
  if (text.includes('ship') || text.includes('deliver') || text.includes('cod'))
    return 'We offer complimentary shipping on orders over PKR 50,000. Cash on delivery, bank transfer, and card are available at checkout.'
  if (text.includes('pay') || text.includes('card') || text.includes('bank'))
    return 'In your bag you can choose Card, Bank transfer, or Cash on delivery. Demo checkout saves your preference without collecting card numbers on-page.'
  if (text.includes('return') || text.includes('exchange'))
    return 'Exchanges are considered within 7 days for unworn pieces with original packaging. Message us on WhatsApp for a guided return.'
  if (text.includes('hello') || text.includes('hi') || text.includes('salam'))
    return 'Welcome to WristMatch. Ask about the collection, try-on studio, shipping, or payment — or tap WhatsApp below to speak with a person.'
  return 'I can help with collection browsing, private try-on, shipping, and payment options. For a personal recommendation, WhatsApp or call us — links are in the footer.'
}

export default function ShopperLayout() {
  const location = useLocation()
  const [open, setOpen] = useState(false)
  const [bagOpen, setBagOpen] = useState(false)
  const [chatOpen, setChatOpen] = useState(false)
  const [draft, setDraft] = useState('')
  const [messages, setMessages] = useState<ChatMessage[]>([
    { role: 'bot', text: 'Hello. I am the WristMatch guide. Ask about watches, try-on, shipping, or payment.' },
  ])
  const chatEnd = useRef<HTMLDivElement>(null)
  const { favourites, cart, cartCount, setCartQuantity, removeFromCart } = useStore()
  const { data } = useQuery({ queryKey: ['header-bag-products'], queryFn: () => getProducts('?page=1&pageSize=50') })

  const bagLines = useMemo(() => {
    const products = data?.items ?? []
    return cart.map(line => {
      const product = products.find(item => item.id === line.id)
      return product ? { ...line, product } : null
    }).filter(Boolean) as { id: string; quantity: number; product: NonNullable<typeof data>['items'][number] }[]
  }, [cart, data])

  const bagSubtotal = bagLines.reduce((sum, line) => sum + line.product.price * line.quantity, 0)
  const bagCurrency = bagLines[0]?.product.currency ?? 'PKR'

  const nav = [
    { to: '/catalog', label: 'Collection' },
    { to: '/look-match', label: 'Look Match' },
    { to: '/studio', label: 'Try on' },
    { to: '/compare', label: 'Compare' },
    { to: '/favourites', label: 'Favourites' },
    { to: '/about', label: 'About' },
    { to: '/contact', label: 'Contact' },
    { to: '/account', label: 'Account' },
  ]

  useEffect(() => {
    if (chatOpen) chatEnd.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages, chatOpen])

  useEffect(() => {
    setBagOpen(false)
    setOpen(false)
  }, [location.pathname])

  useEffect(() => {
    document.body.style.overflow = bagOpen ? 'hidden' : ''
    return () => { document.body.style.overflow = '' }
  }, [bagOpen])

  const sendChat = (event: FormEvent) => {
    event.preventDefault()
    const text = draft.trim()
    if (!text) return
    setDraft('')
    setMessages(current => [...current, { role: 'user', text }, { role: 'bot', text: replyTo(text) }])
  }

  return (
    <div className="shopper-shell">
      <header className="store-header">
        <Link className="brand" to="/">WRIST<span>MATCH</span></Link>
        <nav className={open ? 'store-nav open' : 'store-nav'}>
          {nav.map(item => <NavLink key={item.to} to={item.to} onClick={() => setOpen(false)}>{item.label}</NavLink>)}
        </nav>
        <div className="header-actions">
          <Link to="/favourites" className="header-action" aria-label="Favourites"><Heart size={19} /><span>{favourites.length}</span></Link>
          <button
            type="button"
            className={cartCount > 0 ? 'header-action cart-action active' : 'header-action cart-action'}
            aria-label="Open shopping bag"
            aria-expanded={bagOpen}
            onClick={() => setBagOpen(true)}
          >
            <ShoppingBag size={19} />
            <span>{cartCount}</span>
          </button>
          <button className="mobile-menu" onClick={() => setOpen(value => !value)} aria-label={open ? 'Close menu' : 'Open menu'}>{open ? <X size={21} /> : <Menu size={21} />}</button>
        </div>
      </header>

      <Outlet />

      {bagOpen && (
        <div className="mini-bag" role="dialog" aria-label="Your bag">
          <button type="button" className="mini-bag-backdrop" aria-label="Close bag" onClick={() => setBagOpen(false)} />
          <aside className="mini-bag-panel">
            <header className="mini-bag-head">
              <div>
                <p className="eyebrow">YOUR BAG</p>
                <strong>{cartCount} {cartCount === 1 ? 'item' : 'items'}</strong>
              </div>
              <button type="button" onClick={() => setBagOpen(false)} aria-label="Close"><X size={18} /></button>
            </header>

            {bagLines.length === 0 ? (
              <div className="mini-bag-empty">
                <ShoppingBag size={28} />
                <h2>Your bag is empty</h2>
                <Link className="primary-button compact-button" to="/catalog" onClick={() => setBagOpen(false)}>Browse watches</Link>
              </div>
            ) : (
              <>
                <div className="mini-bag-lines">
                  {bagLines.map(line => (
                    <article key={line.id} className="mini-bag-line">
                      <Link to={`/products/${line.id}`} onClick={() => setBagOpen(false)}>
                        <img src={line.product.imageUrl} alt={line.product.name} />
                      </Link>
                      <div>
                        <Link to={`/products/${line.id}`} onClick={() => setBagOpen(false)}><h3>{line.product.name}</h3></Link>
                        <p><i className="colour-dot inline" style={{ background: colourHex(line.product.colour) }} />{line.product.colour}</p>
                        <strong>{line.product.currency} {(line.product.price * line.quantity).toLocaleString()}</strong>
                        <div className="mini-bag-qty">
                          <div className="qty-control">
                            <button type="button" aria-label="Decrease" onClick={() => setCartQuantity(line.id, line.quantity - 1)}><Minus size={14} /></button>
                            <span>{line.quantity}</span>
                            <button type="button" aria-label="Increase" onClick={() => setCartQuantity(line.id, line.quantity + 1)}><Plus size={14} /></button>
                          </div>
                          <button type="button" className="remove-button" onClick={() => removeFromCart(line.id)} aria-label={`Remove ${line.product.name}`}><Trash2 size={15} /></button>
                        </div>
                      </div>
                    </article>
                  ))}
                </div>
                <footer className="mini-bag-foot">
                  <div className="mini-bag-subtotal">
                    <span>Subtotal</span>
                    <strong>{bagCurrency} {bagSubtotal.toLocaleString()}</strong>
                  </div>
                  <Link className="primary-button mini-bag-checkout" to="/cart" onClick={() => setBagOpen(false)}>
                    Checkout <ArrowRight size={16} />
                  </Link>
                  <button type="button" className="mini-bag-continue" onClick={() => setBagOpen(false)}>Continue shopping</button>
                </footer>
              </>
            )}
          </aside>
        </div>
      )}

      <footer className="store-footer colorful">
        <div className="footer-accent-bar" aria-hidden="true" />
        <div className="footer-grid">
          <div className="footer-brand">
            <Link className="brand" to="/">WRIST<span>MATCH</span></Link>
            <p>Thoughtful timepieces and private visual previews — chosen slowly, decided with confidence.</p>
            <div className="footer-contact">
              <a className="footer-contact-btn whatsapp" href={`https://wa.me/${WHATSAPP}`} target="_blank" rel="noreferrer">
                <MessageCircle size={17} /> WhatsApp
              </a>
              <a className="footer-contact-btn phone" href={`tel:${PHONE_TEL}`}>
                <Phone size={17} /> {PHONE_DISPLAY}
              </a>
            </div>
            <div className="footer-social">
              <a className="ig" href="https://instagram.com" target="_blank" rel="noreferrer" aria-label="Instagram">
                <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8"><rect x="3" y="3" width="18" height="18" rx="5"/><circle cx="12" cy="12" r="4"/><circle cx="17.5" cy="6.5" r="1" fill="currentColor" stroke="none"/></svg>
              </a>
              <a className="wa" href={`https://wa.me/${WHATSAPP}`} target="_blank" rel="noreferrer" aria-label="WhatsApp"><MessageCircle size={17} /></a>
              <a className="mail" href={`mailto:${EMAIL}`} aria-label="Email"><Mail size={17} /></a>
              <a className="call" href={`tel:${PHONE_TEL}`} aria-label="Phone"><Phone size={17} /></a>
            </div>
          </div>

          <div className="footer-col">
            <h3>Shop</h3>
            <Link to="/catalog">Collection</Link>
            <Link to="/catalog?occasion=Gift">Gift edit</Link>
            <Link to="/cart">Your bag</Link>
            <Link to="/favourites">Favourites</Link>
          </div>

          <div className="footer-col">
            <h3>Company</h3>
            <Link to="/about">About us</Link>
            <Link to="/contact">Contact us</Link>
            <Link to="/account">My account</Link>
            <Link to="/studio">Try-on studio</Link>
            <Link to="/compare">Compare</Link>
            <Link to="/privacy">Privacy</Link>
          </div>

          <div className="footer-col footer-visit">
            <h3>Visit</h3>
            <p className="footer-hours">Shop 12, The Forum</p>
            <p className="footer-hours">Clifton Block 9, Karachi</p>
            <p className="footer-hours">Mon–Sat · 11:00–20:00 PKT</p>
            <a href={`mailto:${EMAIL}`}>{EMAIL}</a>
            <Link to="/admin/login">Operations login</Link>
          </div>
        </div>

        <div className="footer-bottom">
          <p>© 2026 WristMatch. Visual previews are style guides, not fit guarantees.</p>
          <div className="footer-bottom-links">
            <Link to="/about">About</Link>
            <Link to="/contact">Contact</Link>
            <Link to="/privacy">Privacy</Link>
            <a href={`https://wa.me/${WHATSAPP}`} target="_blank" rel="noreferrer">WhatsApp</a>
          </div>
        </div>
      </footer>

      <div className={`store-chat ${chatOpen ? 'open' : ''}`}>
        {chatOpen && (
          <div className="store-chat-panel" role="dialog" aria-label="WristMatch AI guide">
            <div className="store-chat-head">
              <div>
                <p className="eyebrow">WRISTMATCH GUIDE</p>
                <strong>Ask anything</strong>
              </div>
              <button onClick={() => setChatOpen(false)} aria-label="Close chat"><X size={18} /></button>
            </div>
            <div className="store-chat-messages">
              {messages.map((message, index) => (
                <div key={`${message.role}-${index}`} className={message.role === 'bot' ? 'chat-bubble bot' : 'chat-bubble user'}>{message.text}</div>
              ))}
              <div ref={chatEnd} />
            </div>
            <div className="store-chat-quick">
              {['Try-on help', 'Shipping', 'Payment options'].map(item => (
                <button key={item} type="button" onClick={() => setMessages(current => [...current, { role: 'user', text: item }, { role: 'bot', text: replyTo(item) }])}>{item}</button>
              ))}
            </div>
            <form className="store-chat-form" onSubmit={sendChat}>
              <input value={draft} onChange={event => setDraft(event.target.value)} placeholder="Type your question..." aria-label="Chat message" />
              <button type="submit" aria-label="Send"><Send size={16} /></button>
            </form>
            <div className="store-chat-actions">
              <a href={`https://wa.me/${WHATSAPP}`} target="_blank" rel="noreferrer"><MessageCircle size={15} /> WhatsApp</a>
              <a href={`tel:${PHONE_TEL}`}><Phone size={15} /> Call</a>
            </div>
          </div>
        )}
        <button className="store-chat-launch" onClick={() => setChatOpen(value => !value)} aria-label={chatOpen ? 'Close AI chat' : 'Open AI chat'}>
          {chatOpen ? <X size={22} /> : <MessageCircle size={22} />}
          {!chatOpen && <span>Ask AI</span>}
        </button>
      </div>
    </div>
  )
}
