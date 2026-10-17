import { Check, Mail, MapPin, MessageCircle, Phone, Send } from 'lucide-react'
import { useState, type FormEvent } from 'react'

const WHATSAPP = '923001234567'
const PHONE_DISPLAY = '+92 300 123 4567'
const PHONE_TEL = '+923001234567'
const EMAIL = 'hello@wristmatch.local'
const ADDRESS = 'Shop 12, The Forum, Clifton Block 9, Karachi'
const MAP_SRC = 'https://www.google.com/maps?q=The+Forum+Clifton+Karachi&output=embed'

type ContactMessage = {
  id: string
  name: string
  email: string
  phone: string
  topic: string
  message: string
  createdAt: string
}

export default function ContactPage() {
  const [form, setForm] = useState({ name: '', email: '', phone: '', topic: 'General', message: '' })
  const [status, setStatus] = useState('')
  const [sending, setSending] = useState(false)

  const submit = (event: FormEvent) => {
    event.preventDefault()
    if (!form.name.trim() || !form.email.trim() || !form.message.trim()) {
      setStatus('Please complete name, email, and message.')
      return
    }
    setSending(true)
    const entry: ContactMessage = {
      id: crypto.randomUUID(),
      name: form.name.trim(),
      email: form.email.trim(),
      phone: form.phone.trim(),
      topic: form.topic,
      message: form.message.trim(),
      createdAt: new Date().toISOString(),
    }
    try {
      const existing = JSON.parse(localStorage.getItem('wristmatch.contactMessages') ?? '[]') as ContactMessage[]
      localStorage.setItem('wristmatch.contactMessages', JSON.stringify([entry, ...existing].slice(0, 50)))
      setForm({ name: '', email: '', phone: '', topic: 'General', message: '' })
      setStatus('Message sent. We will reply within one business day.')
    } catch {
      setStatus('Unable to save your message on this device.')
    } finally {
      setSending(false)
    }
  }

  return (
    <main className="contact-page">
      <section className="contact-hero">
        <p className="eyebrow">CONTACT</p>
        <h1>Talk to WristMatch</h1>
      </section>

      <div className="contact-layout">
        <section className="contact-details">
          <article>
            <Phone size={18} />
            <div>
              <h3>Call</h3>
              <a href={`tel:${PHONE_TEL}`}>{PHONE_DISPLAY}</a>
              <p>Mon–Sat · 11:00–20:00 PKT</p>
            </div>
          </article>
          <article>
            <MessageCircle size={18} />
            <div>
              <h3>WhatsApp</h3>
              <a href={`https://wa.me/${WHATSAPP}`} target="_blank" rel="noreferrer">Chat with us</a>
              <p>Styling advice and order help</p>
            </div>
          </article>
          <article>
            <Mail size={18} />
            <div>
              <h3>Email</h3>
              <a href={`mailto:${EMAIL}`}>{EMAIL}</a>
              <p>Replies within one business day</p>
            </div>
          </article>
          <article>
            <MapPin size={18} />
            <div>
              <h3>Studio</h3>
              <p>{ADDRESS}</p>
            </div>
          </article>
          <div className="contact-social">
            <a href="https://instagram.com" target="_blank" rel="noreferrer" aria-label="Instagram">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8"><rect x="3" y="3" width="18" height="18" rx="5"/><circle cx="12" cy="12" r="4"/><circle cx="17.5" cy="6.5" r="1" fill="currentColor" stroke="none"/></svg>
            </a>
            <a href={`https://wa.me/${WHATSAPP}`} target="_blank" rel="noreferrer" aria-label="WhatsApp"><MessageCircle size={18} /></a>
            <a href={`mailto:${EMAIL}`} aria-label="Email"><Mail size={18} /></a>
            <a href={`tel:${PHONE_TEL}`} aria-label="Phone"><Phone size={18} /></a>
          </div>
        </section>

        <form className="contact-form" onSubmit={submit}>
          <p className="eyebrow">SEND A MESSAGE</p>
          <h2>Write to us</h2>
          <div className="contact-form-grid">
            <label>Name<input required value={form.name} onChange={event => setForm(current => ({ ...current, name: event.target.value }))} /></label>
            <label>Email<input required type="email" value={form.email} onChange={event => setForm(current => ({ ...current, email: event.target.value }))} /></label>
            <label>Phone<input value={form.phone} onChange={event => setForm(current => ({ ...current, phone: event.target.value }))} /></label>
            <label>Topic
              <select value={form.topic} onChange={event => setForm(current => ({ ...current, topic: event.target.value }))}>
                <option>General</option>
                <option>Orders</option>
                <option>Try-on help</option>
                <option>Wholesale</option>
              </select>
            </label>
          </div>
          <label>Message<textarea required rows={5} value={form.message} onChange={event => setForm(current => ({ ...current, message: event.target.value }))} /></label>
          {status && <p className="contact-status"><Check size={15} /> {status}</p>}
          <button className="primary-button" type="submit" disabled={sending}><Send size={16} /> {sending ? 'Sending...' : 'Send message'}</button>
        </form>
      </div>

      <section className="contact-map">
        <div className="contact-map-copy">
          <p className="eyebrow">FIND US</p>
          <h2>Clifton, Karachi</h2>
          <p>{ADDRESS}</p>
        </div>
        <div className="contact-map-frame">
          <iframe title="WristMatch location map" src={MAP_SRC} loading="lazy" referrerPolicy="no-referrer-when-downgrade" />
        </div>
      </section>
    </main>
  )
}
