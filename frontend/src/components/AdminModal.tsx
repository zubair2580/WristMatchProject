import { X } from 'lucide-react'
import { useEffect, type ReactNode } from 'react'

export default function AdminModal({
  title,
  eyebrow,
  onClose,
  children,
  footer,
  wide = false,
}: {
  title: string
  eyebrow: string
  onClose: () => void
  children: ReactNode
  footer: ReactNode
  wide?: boolean
}) {
  useEffect(() => {
    const close = (event: KeyboardEvent) => { if (event.key === 'Escape') onClose() }
    document.addEventListener('keydown', close)
    document.body.style.overflow = 'hidden'
    return () => {
      document.removeEventListener('keydown', close)
      document.body.style.overflow = ''
    }
  }, [onClose])

  return (
    <div className="modal-backdrop" role="presentation" onMouseDown={event => { if (event.target === event.currentTarget) onClose() }}>
      <section className={wide ? 'admin-modal admin-modal-wide' : 'admin-modal'} role="dialog" aria-modal="true" aria-labelledby="modal-title">
        <header>
          <div>
            <p className="eyebrow">{eyebrow}</p>
            <h2 id="modal-title">{title}</h2>
          </div>
          <button onClick={onClose} aria-label="Close dialog"><X size={19} /></button>
        </header>
        <div className="admin-modal-body">{children}</div>
        <footer>{footer}</footer>
      </section>
    </div>
  )
}
