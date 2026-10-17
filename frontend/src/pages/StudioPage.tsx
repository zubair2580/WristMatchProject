import { ArrowRight, Camera, Check, ImagePlus, RefreshCw, ShieldCheck, Sparkles, Trash2, Upload } from 'lucide-react'
import { useEffect, useMemo, useRef, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { apiUrl, getProducts } from '../store/api'
import { colourHex } from '../store/productExtras'
import { useStore } from '../store/StoreContext'

type UploadedPhoto = { id: string; expiresAt: string; previewUrl: string; width: number; height: number; byteSize: number }
type TryOnJob = { id: string; productId: string; state: string; providerMode: string; safeMessage?: string | null; expiresAt: string; resultUrl?: string | null }

export default function StudioPage() {
  const inputRef = useRef<HTMLInputElement>(null)
  const [searchParams] = useSearchParams()
  const { selection, addToCart } = useStore()
  const [file, setFile] = useState<File | null>(null)
  const [consent, setConsent] = useState(false)
  const [photo, setPhoto] = useState<UploadedPhoto | null>(null)
  const [productId, setProductId] = useState(searchParams.get('product') ?? '')
  const [job, setJob] = useState<TryOnJob | null>(null)
  const [error, setError] = useState('')
  const [uploading, setUploading] = useState(false)
  const [generating, setGenerating] = useState(false)
  const localPreview = useMemo(() => file ? URL.createObjectURL(file) : null, [file])
  useEffect(() => () => { if (localPreview) URL.revokeObjectURL(localPreview) }, [localPreview])

  const catalogue = useQuery({
    queryKey: ['studio-products'],
    queryFn: () => getProducts('?page=1&pageSize=50&sort=recommended'),
  })
  const providerStatus = useQuery({
    queryKey: ['youcam-status'],
    queryFn: async () => {
      const response = await fetch(`${apiUrl}/look-match/status`)
      if (!response.ok) throw new Error('status')
      return response.json() as Promise<{ youCamConfigured: boolean; tryOnProviderMode: string }>
    },
  })
  const products = catalogue.data?.items ?? []
  const selectedProducts = products.filter(item => selection.includes(item.id))
  const choices = selectedProducts.length ? selectedProducts : products.slice(0, 8)
  const liveYouCam = providerStatus.data?.tryOnProviderMode === 'youcam'

  useEffect(() => {
    const fromQuery = searchParams.get('product')
    if (fromQuery) setProductId(fromQuery)
  }, [searchParams])

  useEffect(() => {
    if (!productId && choices.length) setProductId(choices[0].id)
  }, [choices, productId])

  useEffect(() => {
    if (!job || job.state === 'Succeeded' || job.state === 'Failed' || job.state === 'Expired') return
    const timer = window.setInterval(async () => {
      const response = await fetch(`${apiUrl}/tryon-jobs/${job.id}`, { credentials: 'include' })
      if (!response.ok) return
      const next = await response.json() as TryOnJob
      setJob(next)
      if (next.state === 'Succeeded' || next.state === 'Failed' || next.state === 'Expired') setGenerating(false)
    }, 1500)
    return () => window.clearInterval(timer)
  }, [job])

  const chooseFile = (selected: File | null) => {
    setError('')
    setPhoto(null)
    setJob(null)
    if (!selected) return
    if (!['image/jpeg', 'image/png'].includes(selected.type)) { setError('Choose a JPEG or PNG photo.'); return }
    if (selected.size > 8 * 1024 * 1024) { setError('The photo must be no larger than 8 MiB.'); return }
    setFile(selected)
  }

  const upload = async () => {
    if (!file || !consent) { setError('Choose a photo and confirm consent before continuing.'); return }
    setUploading(true)
    setError('')
    try {
      const session = await fetch(`${apiUrl}/sessions`, { method: 'POST', credentials: 'include' })
      if (!session.ok) throw new Error('A private session could not be created.')
      const body = new FormData()
      body.append('file', file)
      body.append('consent', 'true')
      body.append('consentVersion', '2026-10-11')
      const response = await fetch(`${apiUrl}/photos`, { method: 'POST', credentials: 'include', body })
      if (!response.ok) {
        const problem = await response.json().catch(() => null) as { detail?: string } | null
        throw new Error(problem?.detail ?? 'The photo could not be uploaded.')
      }
      setPhoto(await response.json() as UploadedPhoto)
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'The photo could not be uploaded.')
    } finally {
      setUploading(false)
    }
  }

  const generate = async () => {
    if (!photo || !productId) { setError('Secure a photo and choose a watch before generating.'); return }
    setGenerating(true)
    setError('')
    try {
      const idempotencyKey = crypto.randomUUID()
      const response = await fetch(`${apiUrl}/tryon-jobs`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json', 'Idempotency-Key': idempotencyKey },
        body: JSON.stringify({ photoId: photo.id, productId, idempotencyKey }),
      })
      if (!response.ok) {
        const problem = await response.json().catch(() => null) as { detail?: string } | null
        throw new Error(problem?.detail ?? 'The preview could not be started.')
      }
      setJob(await response.json() as TryOnJob)
    } catch (exception) {
      setGenerating(false)
      setError(exception instanceof Error ? exception.message : 'The preview could not be started.')
    }
  }

  const removePhoto = async () => {
    if (photo) await fetch(`${apiUrl}/photos/${photo.id}`, { method: 'DELETE', credentials: 'include' })
    setPhoto(null); setFile(null); setConsent(false); setJob(null); setError('')
  }

  const removeResult = async () => {
    if (job) await fetch(`${apiUrl}/tryon-jobs/${job.id}`, { method: 'DELETE', credentials: 'include' })
    setJob(null)
  }

  const imageUrl = job?.state === 'Succeeded'
    ? `${apiUrl}/tryon-jobs/${job.id}/result`
    : photo ? `${apiUrl}/photos/${photo.id}/image` : localPreview
  const selected = choices.find(item => item.id === productId) ?? products.find(item => item.id === productId)
  const step = job?.state === 'Succeeded' ? 3 : photo ? 2 : 1
  const providerLabel = (job?.providerMode ?? providerStatus.data?.tryOnProviderMode ?? 'demo').toUpperCase()

  return (
    <main className="studio-page studio-rich">
      <section className="studio-hero-band">
        <div>
          <p className="eyebrow">YOUCAM WATCH VTO · PRIVATE STUDIO</p>
          <h1>See it on <em>your wrist.</em></h1>
        </div>
        <ol className="studio-steps">
          <li className={step >= 1 ? 'active' : ''}><span>01</span> Photo</li>
          <li className={step >= 2 ? 'active' : ''}><span>02</span> Watch</li>
          <li className={step >= 3 ? 'active' : ''}><span>03</span> Preview</li>
        </ol>
      </section>

      <div className={`studio-provider-banner ${liveYouCam ? 'live' : 'demo'}`}>
        <Sparkles size={16} />
        <span>
          {liveYouCam
            ? 'Live YouCam AI Watch Virtual Try-On is enabled. Wrist photos are uploaded server-side to Perfect Corp and never stored in the browser.'
            : 'Demo mode is active. Set YouCam__ApiKey (user-secrets or env) and TryOn:ProviderMode=youcam for live Perfect Corp Watch VTO.'}
        </span>
        <Link to="/look-match">Look Match first <ArrowRight size={14} /></Link>
      </div>

      <div className="studio-workspace rich">
        <section className={imageUrl ? 'studio-upload has-preview' : 'studio-upload'}>
          {imageUrl ? (
            <div className="studio-preview">
              <img src={imageUrl} alt={job?.state === 'Succeeded' ? 'Generated wrist preview' : 'Your selected wrist'} />
              <div className="preview-badge">
                {job?.state === 'Succeeded'
                  ? `${providerLabel} PREVIEW`
                  : photo ? 'PRIVATE PHOTO READY' : 'LOCAL PREVIEW'}
              </div>
            </div>
          ) : (
            <div className="studio-drop">
              <div className="upload-icon"><ImagePlus size={26} /></div>
              <h2>Start with a wrist photo</h2>
              <p>JPEG or PNG · back of wrist · five fingers visible</p>
            </div>
          )}
          <input ref={inputRef} type="file" accept="image/jpeg,image/png" hidden onChange={event => chooseFile(event.target.files?.[0] ?? null)} />
          <div className="upload-actions">
            <button className="outline-button" onClick={() => inputRef.current?.click()}>{file ? <RefreshCw size={16} /> : <Camera size={17} />} {file ? 'Replace photo' : 'Choose a photo'}</button>
            {file && !photo && <button className="primary-button compact-button" onClick={upload} disabled={!consent || uploading}><Upload size={16} /> {uploading ? 'Securing...' : 'Secure photo'}</button>}
            {photo && <button className="remove-photo" onClick={removePhoto}><Trash2 size={16} /> Delete photo</button>}
            {job?.state === 'Succeeded' && <button className="remove-photo" onClick={removeResult}><Trash2 size={16} /> Delete preview</button>}
          </div>
          {file && <div className="file-facts"><span>{file.name}</span><span>{(file.size / 1024 / 1024).toFixed(2)} MiB</span></div>}
          {!photo && file && (
            <label className="consent-card">
              <input type="checkbox" checked={consent} onChange={event => setConsent(event.target.checked)} />
              <span><strong>I have permission to use this photo.</strong> Private try-on only. <Link to="/privacy">Privacy</Link></span>
            </label>
          )}
          {error && <div className="studio-error">{error}</div>}
          {photo && !job && <div className="photo-ready"><Check size={17} /><span>Ready · {photo.width} × {photo.height}</span></div>}
          {job && (
            <div className="photo-ready">
              <Check size={17} />
              <span>{job.state}{selected ? ` · ${selected.name}` : ''}{job.safeMessage ? ` · ${job.safeMessage}` : ''}</span>
            </div>
          )}
          {job?.state === 'Succeeded' && selected && (
            <div className="studio-result-actions">
              <button type="button" className="primary-button compact-button" onClick={() => addToCart(selected.id)}><Sparkles size={16} /> Add to bag</button>
              <Link className="outline-button" to="/compare">Compare</Link>
              <Link className="text-link" to="/cart">Checkout <ArrowRight size={15} /></Link>
            </div>
          )}
        </section>

        <aside className="studio-side">
          <div className="studio-watch-picker">
            <p className="eyebrow">CHOOSE A WATCH</p>
            <h2>{selected?.name ?? 'Select a piece'}</h2>
            <div className="studio-watch-grid">
              {choices.map(item => (
                <button key={item.id} type="button" className={productId === item.id ? 'active' : ''} onClick={() => { setProductId(item.id); setJob(null) }}>
                  <img src={item.imageUrl} alt={item.name} />
                  <span>
                    <strong>{item.name}</strong>
                    <small><i style={{ background: colourHex(item.colour) }} />{item.currency} {item.price.toLocaleString()}</small>
                  </span>
                </button>
              ))}
            </div>
            <button className="primary-button compact-button studio-generate-btn" onClick={generate} disabled={!photo || !productId || generating || job?.state === 'Queued' || job?.state === 'Processing'}>
              <Sparkles size={16} /> {generating || job?.state === 'Queued' || job?.state === 'Processing' ? 'Generating...' : liveYouCam ? 'Generate YouCam preview' : 'Generate demo preview'}
            </button>
          </div>
          <div className="studio-guide compact">
            <p className="eyebrow">YOUCAM WRIST TIPS</p>
            <div><Check size={16} /><span>Back of wrist fully visible</span></div>
            <div><Check size={16} /><span>All five fingers in frame</span></div>
            <div><Check size={16} /><span>Even natural light, no occlusion</span></div>
            <div><Check size={16} /><span>Watch SKU upright, front-facing</span></div>
            <div className="studio-privacy"><ShieldCheck size={17} /><p>Private by default. Credentials stay server-side. Photos expire automatically.</p></div>
          </div>
        </aside>
      </div>

      <div className="studio-disclaimer"><strong>AI-generated visual preview — appearance and fit may vary.</strong> Powered by YouCam API when live.</div>
      <div className="studio-bottom-links">
        <Link className="text-link" to="/look-match">Skin Look Match <ArrowRight size={15} /></Link>
        <Link className="text-link" to="/catalog">Browse watches <ArrowRight size={15} /></Link>
        <Link className="text-link" to="/cart">View bag <ArrowRight size={15} /></Link>
      </div>
    </main>
  )
}
